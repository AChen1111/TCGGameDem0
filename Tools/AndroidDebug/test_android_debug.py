"""Device isolation, command validation, streaming cancellation and reconnect tests."""
from contextlib import redirect_stdout
import io
from pathlib import Path
import subprocess
import sys
import tempfile
import time
import unittest
from unittest.mock import patch

from adb_client import Adb, DebugError, Device, Mapping, find_adb, logcat_args, parse_devices, parse_mappings, tcp_port
import main


class DeviceAndPortTests(unittest.TestCase):
    def test_device_states_and_models(self):
        found = parse_devices("* daemon started successfully *\nList of devices attached\n"
                              "phone1 device product:one model:MEIZU_21 transport_id:1\n"
                              "phone2 unauthorized transport_id:2\nphone3 offline\n")
        self.assertEqual([(x.serial, x.state, x.model) for x in found],
                         [("phone1", "device", "MEIZU 21"), ("phone2", "unauthorized", ""), ("phone3", "offline", "")])

    def test_multi_device_does_not_choose_arbitrarily(self):
        adb = Adb(Path("adb.exe"))
        with patch.object(adb, "devices", return_value=[Device("A", "device"), Device("B", "device")]):
            with self.assertRaises(DebugError):
                adb.select(None)
            self.assertEqual(adb.select("B").serial, "B")

    def test_unauthorized_and_missing_selected_device_are_rejected(self):
        adb = Adb(Path("adb.exe"))
        with patch.object(adb, "devices", return_value=[Device("A", "unauthorized")]):
            for serial in ("A", "B", None):
                with self.subTest(serial=serial), self.assertRaises(DebugError):
                    adb.select(serial)

    def test_single_ready_device_is_selected(self):
        adb = Adb(Path("adb.exe"))
        with patch.object(adb, "devices", return_value=[Device("A", "offline"), Device("B", "device")]):
            self.assertEqual(adb.select(None).serial, "B")

    def test_forward_listing_is_scoped_to_selected_phone(self):
        result = parse_mappings("A tcp:5080 tcp:5080\nB tcp:9000 tcp:9001\n", "forward", "B")
        self.assertEqual(result, [Mapping("forward", "tcp:9000", "tcp:9001")])

    def test_reverse_transport_label_is_not_a_device_serial(self):
        result = parse_mappings("UsbFfs tcp:5080 tcp:5080\n", "reverse", "phone")
        self.assertEqual(result, [Mapping("reverse", "tcp:5080", "tcp:5080")])

    def test_mapping_arguments_and_removal_leave_other_devices_alone(self):
        adb = Adb(Path("adb.exe"))
        with patch.object(adb, "run") as run:
            adb.add_mapping("B", "reverse", 8080, 5080)
            run.assert_called_once_with("reverse", "tcp:8080", "tcp:5080", serial="B")
            run.reset_mock()
            adb.remove_mapping("B", Mapping("reverse", "tcp:8080", "tcp:5080"))
            run.assert_called_once_with("reverse", "--remove", "tcp:8080", serial="B")

    def test_port_validation_blocks_bad_values_and_shell_text(self):
        self.assertEqual(tcp_port("65535"), 65535)
        for value in ("0", "65536", "-1", "abc", "5080 & whoami", "５０８０", "3.5"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                tcp_port(value)

    def test_explicit_invalid_adb_does_not_fall_back(self):
        with self.assertRaises(DebugError):
            find_adb("/this-adb-does-not-exist-304923.exe")

    def test_log_filters_include_native_and_java_crash_without_pid_binding(self):
        args = logcat_args("unity")
        self.assertIn("-T", args)
        self.assertNotIn("-t", args)
        for token in ("main", "system", "crash", "Unity:V", "AndroidRuntime:E", "DEBUG:F", "CRASH:E"):
            self.assertIn(token, args)
        self.assertFalse(any("--pid" in token for token in args))
        self.assertEqual(logcat_args("crash", dump=True), ["logcat", "-b", "crash", "-v", "threadtime", "-d", "*:V"])

    def test_cli_mapping_defaults_and_direction(self):
        args = main.parser().parse_args(["--serial", "B", "reverse", "8080", "5080"])
        self.assertEqual((args.serial, args.action, args.source, args.target), ("B", "reverse", 8080, 5080))
        self.assertEqual(main.parser().parse_args(["reverse"]).source, 5080)

    def test_log_name_cannot_escape_directory(self):
        name = main.log_path("../phone:5555", "unity")
        self.assertEqual(name.parent, main.LOG_DIR)
        self.assertNotIn(":", name.name)
        self.assertNotIn("/", name.name)


class StreamingTests(unittest.TestCase):
    def fake_adb(self):
        class Fake:
            def command(self, *args, **kwargs):
                return [sys.executable, "-X", "utf8", "-u", "-c", "import time; print('Unity 中文日志', flush=True); time.sleep(60)"]
        return Fake()

    def test_duration_stops_only_owned_logcat_and_flushes_file(self):
        processes = []
        real_popen = subprocess.Popen
        def capture(*args, **kwargs):
            proc = real_popen(*args, **kwargs)
            processes.append(proc)
            return proc
        with tempfile.TemporaryDirectory() as temporary, patch.object(main.subprocess, "Popen", side_effect=capture):
            path = Path(temporary) / "capture.log"
            with redirect_stdout(io.StringIO()):
                main.stream_once(self.fake_adb(), Device("B", "device"), "unity", path, 200, time.monotonic() + 0.5)
            self.assertIsNotNone(processes[0].poll())
            self.assertIn("Unity 中文日志", path.read_text(encoding="utf-8"))

    def test_ctrl_c_terminates_child_process(self):
        processes = []
        real_popen = subprocess.Popen
        def capture(*args, **kwargs):
            proc = real_popen(*args, **kwargs)
            processes.append(proc)
            return proc
        with tempfile.TemporaryDirectory() as temporary, patch.object(main.subprocess, "Popen", side_effect=capture):
            with patch("builtins.print", side_effect=KeyboardInterrupt), self.assertRaises(KeyboardInterrupt):
                main.stream_once(self.fake_adb(), Device("B", "device"), "unity", Path(temporary) / "capture.log", 200, None)
            self.assertIsNotNone(processes[0].poll())

    def test_reconnect_keeps_same_phone(self):
        adb = Adb(Path("adb.exe"))
        with tempfile.TemporaryDirectory() as temporary, redirect_stdout(io.StringIO()):
            with patch.object(main, "stream_once", side_effect=[1, 0]) as stream:
                with patch.object(adb, "run", side_effect=["offline", "device"]) as run:
                    main.watch_logs(adb, Device("B", "device"), "unity", output=Path(temporary) / "capture.log")
                self.assertEqual(stream.call_count, 2)
                self.assertTrue(all(call.args[1].serial == "B" for call in stream.call_args_list))
                self.assertTrue(all(call.kwargs["serial"] == "B" for call in run.call_args_list))

    def test_live_logcat_failure_does_not_reconnect_forever(self):
        adb = Adb(Path("adb.exe"))
        with tempfile.TemporaryDirectory() as temporary, redirect_stdout(io.StringIO()):
            with patch.object(main, "stream_once", return_value=1), patch.object(adb, "run", return_value="device"):
                with self.assertRaises(DebugError):
                    main.watch_logs(adb, Device("B", "device"), "unity", output=Path(temporary) / "capture.log")


if __name__ == "__main__":
    unittest.main()
