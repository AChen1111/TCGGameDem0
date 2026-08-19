import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import "monaco-editor/esm/vs/basic-languages/lua/lua.contribution";
import "monaco-editor/esm/vs/editor/contrib/suggest/browser/suggestController";
import "monaco-editor/esm/vs/editor/contrib/parameterHints/browser/parameterHints";
import editorWorker from "monaco-editor/esm/vs/editor/editor.worker?worker";

self.MonacoEnvironment = {
  getWorker: () => new editorWorker(),
};

type SourceItem = {
  label?: string;
  insertText?: string;
  kind?: monaco.languages.CompletionItemKind;
  detail?: string;
  documentation?: string;
};

const luaSyntax: SourceItem[] = [
  { label: "and", insertText: "and" },
  { label: "break", insertText: "break" },
  { label: "do", insertText: "do" },
  { label: "else", insertText: "else" },
  { label: "elseif", insertText: "elseif ${1:condition} then", kind: monaco.languages.CompletionItemKind.Snippet, detail: "elseif condition then" },
  { label: "end", insertText: "end" },
  { label: "false", insertText: "false" },
  { label: "for", insertText: "for ${1:i} = ${2:1}, ${3:10} do\n\t$0\nend", kind: monaco.languages.CompletionItemKind.Snippet, detail: "for i = 1, 10 do .. end" },
  { label: "function", insertText: "function ${1:name}(${2:})\n\t$0\nend", kind: monaco.languages.CompletionItemKind.Snippet, detail: "function name() .. end" },
  { label: "goto", insertText: "goto" },
  { label: "if", insertText: "if ${1:condition} then\n\t$0\nend", kind: monaco.languages.CompletionItemKind.Snippet, detail: "if condition then .. end" },
  { label: "in", insertText: "in" },
  { label: "ipairs", insertText: "ipairs()" },
  { label: "local", insertText: "local" },
  { label: "nil", insertText: "nil" },
  { label: "not", insertText: "not" },
  { label: "or", insertText: "or" },
  { label: "pairs", insertText: "pairs()" },
  { label: "pcall", insertText: "pcall()" },
  { label: "print", insertText: "print()" },
  { label: "repeat", insertText: "repeat\n\t$0\nuntil ${1:condition}", kind: monaco.languages.CompletionItemKind.Snippet, detail: "repeat .. until condition" },
  { label: "require", insertText: "require(\"\")" },
  { label: "return", insertText: "return" },
  { label: "then", insertText: "then" },
  { label: "true", insertText: "true" },
  { label: "type", insertText: "type()" },
  { label: "until", insertText: "until" },
  { label: "while", insertText: "while ${1:condition} do\n\t$0\nend", kind: monaco.languages.CompletionItemKind.Snippet, detail: "while condition do .. end" },
];

async function rpc(msg: unknown): Promise<HostPayload> {
  const r = await fetch("/rpc", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(msg),
  });
  return r.json();
}

const editor = monaco.editor.create(document.getElementById("app")!, {
  value: "print('hello from Lua Pad')\n",
  language: "lua",
  theme: "vs-dark",
  automaticLayout: true,
  minimap: { enabled: false },
  fontSize: 18,
  lineHeight: 26,
  fontFamily: "Cascadia Code, Consolas, Courier New, monospace",
  fontLigatures: true,
  lineNumbers: "on",
  glyphMargin: true,
  renderLineHighlight: "line",
  scrollBeyondLastLine: false,
  padding: { top: 8, bottom: 8 },
  tabSize: 4,
  insertSpaces: true,
  cursorBlinking: "smooth",
  roundedSelection: false,
  quickSuggestions: { other: true, comments: false, strings: false },
  suggestOnTriggerCharacters: true,
  wordBasedSuggestions: "off",
  snippetSuggestions: "top",
  parameterHints: { enabled: true },
});

const output = document.getElementById("output")!;
let startLine = 1;
let endLine = editor.getModel()!.getLineCount();
let decos: string[] = [];
let drag: "start" | "end" | null = null;

function clampRange() {
  const last = editor.getModel()!.getLineCount();
  if (last < 2) {
    startLine = 1;
    endLine = last;
    return;
  }
  if (endLine > last) endLine = last;
  if (endLine < 2) endLine = 2;
  if (startLine < 1) startLine = 1;
  if (startLine > endLine - 1) startLine = endLine - 1;
}

function paintRange() {
  clampRange();
  decos = editor.deltaDecorations(decos, [
    { range: new monaco.Range(startLine, 1, startLine, 1), options: { glyphMarginClassName: "lua-pad-cur-start" } },
    { range: new monaco.Range(endLine, 1, endLine, 1), options: { glyphMarginClassName: "lua-pad-cur-end" } },
    { range: new monaco.Range(startLine, 1, endLine, 1), options: { isWholeLine: true, className: "lua-pad-range" } },
  ]);
}

function applyLine(line: number) {
  if (drag === "start") startLine = Math.min(line, endLine - 1);
  else if (drag === "end") endLine = Math.max(line, startLine + 1);
  paintRange();
}

function isGutter(t: monaco.editor.IMouseTarget) {
  return (
    t.type === monaco.editor.MouseTargetType.GUTTER_GLYPH_MARGIN ||
    t.type === monaco.editor.MouseTargetType.GUTTER_LINE_NUMBERS ||
    t.type === monaco.editor.MouseTargetType.GUTTER_LINE_DECORATIONS
  );
}

editor.onMouseDown((e) => {
  if (!isGutter(e.target) || !e.target.position) return;
  const line = e.target.position.lineNumber;
  const el = e.target.element;
  if (el?.closest(".lua-pad-cur-start")) drag = "start";
  else if (el?.closest(".lua-pad-cur-end")) drag = "end";
  else drag = Math.abs(line - startLine) <= Math.abs(line - endLine) ? "start" : "end";
  applyLine(line);
});
window.addEventListener("mousemove", (ev) => {
  if (!drag) return;
  const t = editor.getTargetAtClientPoint(ev.clientX, ev.clientY);
  if (t?.position) applyLine(t.position.lineNumber);
});
window.addEventListener("mouseup", () => {
  drag = null;
});
paintRange();

document.getElementById("run")!.addEventListener("click", async () => {
  try {
    applyHost(await rpc({ method: "run", text: editor.getValue(), startLine, endLine }));
  } catch (e) {
    output.textContent = String(e);
    output.style.color = "#f48771";
  }
});
document.getElementById("close")!.addEventListener("click", async () => {
  try {
    await rpc({ method: "close" });
  } catch {
  }
  window.close();
});

type HostPayload = {
  id?: string;
  ok?: boolean;
  output?: string;
  error?: string;
  names?: string[];
  text?: string;
  items?: SourceItem[];
  signatures?: monaco.languages.SignatureInformation[];
  activeSignature?: number;
  activeParameter?: number;
  diagnostics?: {
    message?: string;
    severity?: number;
    range?: { start?: { line?: number; character?: number }; end?: { line?: number; character?: number } };
  }[];
};

function applyHost(payload: HostPayload) {
  if (payload.output != null) {
    output.textContent = String(payload.output);
    output.style.color = payload.ok === false ? "#f48771" : "#c8c8c8";
  }
  if (payload.diagnostics && editor.getModel()) {
    monaco.editor.setModelMarkers(
      editor.getModel()!,
      "emmylua",
      payload.diagnostics.map((d) => ({
        message: String(d.message ?? ""),
        severity: Number(d.severity) === 1 ? monaco.MarkerSeverity.Error : monaco.MarkerSeverity.Warning,
        startLineNumber: (d.range?.start?.line ?? 0) + 1,
        startColumn: (d.range?.start?.character ?? 0) + 1,
        endLineNumber: (d.range?.end?.line ?? 0) + 1,
        endColumn: (d.range?.end?.character ?? 0) + 1,
      })),
    );
  }
}

function suggestions(
  model: monaco.editor.ITextModel,
  position: monaco.Position,
  items: SourceItem[],
): monaco.languages.CompletionItem[] {
  const word = model.getWordUntilPosition(position);
  return items.map((it) => {
    let insert = String(it.insertText ?? it.label ?? "");
    if (!insert.includes("${")) {
      if (insert.endsWith("()")) {
        insert = insert.slice(0, -2) + "($0)";
      } else if (insert.endsWith("(\"\")")) {
        insert = insert.slice(0, -4) + "(\"$0\")";
      }
    }
    const label = it.detail
      ? { label: String(it.label ?? ""), detail: " " + it.detail }
      : String(it.label ?? "");
    return {
      label,
      kind: it.kind ?? monaco.languages.CompletionItemKind.Keyword,
      insertText: insert,
      insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
      detail: it.detail,
      documentation: it.documentation,
      range: {
        startLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endLineNumber: position.lineNumber,
        endColumn: position.column,
      },
    };
  });
}

monaco.languages.registerCompletionItemProvider("lua", {
  triggerCharacters: [".", ":"],
  provideCompletionItems: async (model, position) => {
    const word = model.getWordUntilPosition(position);
    const line = model.getLineContent(position.lineNumber);
    const beforeWord = word.startColumn > 1 ? line[word.startColumn - 2] : "";
    const member = beforeWord === "." || beforeWord === ":";
    const local = luaSyntax.filter((it) => !word.word || String(it.label).startsWith(word.word));
    if (!member && !word.word) {
      return { suggestions: suggestions(model, position, local) };
    }
    try {
      const res = await rpc({
        method: "completion",
        text: model.getValue(),
        line: position.lineNumber - 1,
        character: position.column - 1,
      });
      if (res.items && res.items.length > 0) {
        return { suggestions: suggestions(model, position, res.items) };
      }
    } catch {
    }
    return { suggestions: suggestions(model, position, member ? [] : local) };
  },
});

monaco.languages.registerSignatureHelpProvider("lua", {
  signatureHelpTriggerCharacters: ["(", ","],
  provideSignatureHelp: async (model, position) => {
    const empty = {
      value: { signatures: [], activeSignature: 0, activeParameter: 0 },
      dispose: () => {},
    };
    try {
      const res = await rpc({
        method: "signatureHelp",
        text: model.getValue(),
        line: position.lineNumber - 1,
        character: position.column - 1,
      });
      if (!res.signatures || res.signatures.length === 0) {
        return empty;
      }
      return {
        value: {
          signatures: res.signatures,
          activeSignature: res.activeSignature ?? 0,
          activeParameter: res.activeParameter ?? 0,
        },
        dispose: () => {},
      };
    } catch {
      return empty;
    }
  },
});

editor.onDidChangeModelContent(() => {
  paintRange();
  rpc({ method: "changed", text: editor.getValue() })
    .then(applyHost)
    .catch(() => {});
});

const draftList = document.getElementById("draft-list")!;
const emptyDrafts = document.getElementById("empty-drafts")!;
const draftName = document.getElementById("draft-name") as HTMLInputElement;

async function refreshDrafts() {
  const res = await rpc({ method: "draftsList" });
  const names = res.names ?? [];
  draftList.replaceChildren();
  emptyDrafts.hidden = names.length > 0;
  for (const name of names) {
    const btn = document.createElement("button");
    btn.type = "button";
    btn.textContent = name;
    btn.addEventListener("click", async () => {
      const loaded = await rpc({ method: "draftLoad", name });
      if (loaded.text == null) return;
      editor.setValue(String(loaded.text));
      startLine = 1;
      endLine = editor.getModel()!.getLineCount();
      paintRange();
    });
    const li = document.createElement("li");
    li.appendChild(btn);
    draftList.appendChild(li);
  }
}

document.getElementById("save-draft")!.addEventListener("click", async () => {
  const name = draftName.value.trim() || window.prompt("草稿名") || "";
  if (!name) return;
  try {
    const res = await rpc({ method: "draftSave", name, text: editor.getValue() });
    if (res.error) {
      output.textContent = String(res.error);
      output.style.color = "#f48771";
      return;
    }
    if (res.name) draftName.value = String(res.name);
    await refreshDrafts();
  } catch (e) {
    output.textContent = String(e);
    output.style.color = "#f48771";
  }
});

document.getElementById("toggle-drafts")!.addEventListener("click", () => {
  const aside = document.getElementById("drafts")!;
  const collapsed = aside.classList.toggle("collapsed");
  document.getElementById("toggle-drafts")!.setAttribute("aria-expanded", String(!collapsed));
});

refreshDrafts().catch(() => {});
