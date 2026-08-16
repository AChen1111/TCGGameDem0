import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import "monaco-editor/esm/vs/basic-languages/lua/lua.contribution";
import "monaco-editor/esm/vs/editor/contrib/suggest/browser/suggestController";
import editorWorker from "monaco-editor/esm/vs/editor/editor.worker?worker";

self.MonacoEnvironment = {
  getWorker: () => new editorWorker(),
};

type SourceItem = {
  label?: string;
  insertText?: string;
  kind?: monaco.languages.CompletionItemKind;
};

const luaSyntax: SourceItem[] = [
  { label: "and", insertText: "and" },
  { label: "break", insertText: "break" },
  { label: "do", insertText: "do" },
  { label: "else", insertText: "else" },
  { label: "elseif", insertText: "elseif" },
  { label: "end", insertText: "end" },
  { label: "false", insertText: "false" },
  { label: "for", insertText: "for" },
  { label: "function", insertText: "function" },
  { label: "goto", insertText: "goto" },
  { label: "if", insertText: "if" },
  { label: "in", insertText: "in" },
  { label: "ipairs", insertText: "ipairs()" },
  { label: "local", insertText: "local" },
  { label: "nil", insertText: "nil" },
  { label: "not", insertText: "not" },
  { label: "or", insertText: "or" },
  { label: "pairs", insertText: "pairs()" },
  { label: "pcall", insertText: "pcall()" },
  { label: "print", insertText: "print()" },
  { label: "repeat", insertText: "repeat" },
  { label: "require", insertText: "require(\"\")" },
  { label: "return", insertText: "return" },
  { label: "then", insertText: "then" },
  { label: "true", insertText: "true" },
  { label: "type", insertText: "type()" },
  { label: "until", insertText: "until" },
  { label: "while", insertText: "while" },
];

const pending = new Map<string, (items: SourceItem[]) => void>();
let seq = 0;

function post(msg: unknown) {
  const w = window as unknown as {
    chrome?: { webview?: { postMessage: (v: unknown) => void } };
  };
  w.chrome?.webview?.postMessage(JSON.stringify(msg));
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
  snippetSuggestions: "inline",
});

(window as unknown as { luaPadGetText: () => string }).luaPadGetText = () => editor.getValue();
(window as unknown as { luaPadSetFontSize: (n: number) => void }).luaPadSetFontSize = (n) => {
  const size = Math.max(12, n);
  editor.updateOptions({ fontSize: size, lineHeight: Math.round(size * 1.45) });
};

const output = document.getElementById("output")!;
document.getElementById("run")!.addEventListener("click", () => {
  post({ method: "run", text: editor.getValue() });
});
document.getElementById("close")!.addEventListener("click", () => {
  post({ method: "close" });
});

(window as unknown as { luaPadOnHost: (payload: HostPayload) => void }).luaPadOnHost = (payload) => {
  if (payload.output != null) {
    output.textContent = String(payload.output);
    output.style.color = payload.ok === false ? "#f48771" : "#c8c8c8";
  }
  if (payload.items && payload.id && pending.has(payload.id)) {
    const resolve = pending.get(payload.id)!;
    pending.delete(payload.id);
    resolve(payload.items);
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
};

type HostPayload = {
  id?: string;
  ok?: boolean;
  output?: string;
  items?: SourceItem[];
  diagnostics?: {
    message?: string;
    severity?: number;
    range?: { start?: { line?: number; character?: number }; end?: { line?: number; character?: number } };
  }[];
};

function suggestions(
  model: monaco.editor.ITextModel,
  position: monaco.Position,
  items: SourceItem[],
): monaco.languages.CompletionItem[] {
  const word = model.getWordUntilPosition(position);
  return items.map((it) => {
    let insert = String(it.insertText ?? it.label ?? "");
    if (insert.endsWith("()")) {
      insert = insert.slice(0, -2) + "($0)";
    } else if (insert.endsWith("(\"\")")) {
      insert = insert.slice(0, -4) + "(\"$0\")";
    }
    return {
      label: String(it.label ?? ""),
      kind: it.kind ?? monaco.languages.CompletionItemKind.Keyword,
      insertText: insert,
      insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
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
  provideCompletionItems: (model, position) => {
    const word = model.getWordUntilPosition(position);
    const line = model.getLineContent(position.lineNumber);
    const beforeWord = word.startColumn > 1 ? line[word.startColumn - 2] : "";
    if (beforeWord !== "." && beforeWord !== ":") {
      const prefix = word.word;
      return {
        suggestions: suggestions(
          model,
          position,
          luaSyntax.filter((it) => String(it.label).startsWith(prefix)),
        ),
      };
    }
    return new Promise((resolve) => {
      const id = String(++seq);
      pending.set(id, (items) => {
        resolve({ suggestions: suggestions(model, position, items) });
      });
      post({
        method: "completion",
        id,
        text: model.getValue(),
        line: position.lineNumber - 1,
        character: position.column - 1,
      });
      setTimeout(() => {
        if (pending.has(id)) {
          pending.delete(id);
          resolve({ suggestions: [] });
        }
      }, 12000);
    });
  },
});

editor.onDidChangeModelContent(() => {
  post({ method: "changed", text: editor.getValue() });
});
