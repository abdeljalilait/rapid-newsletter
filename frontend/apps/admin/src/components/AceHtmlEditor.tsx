import { useEffect, useRef } from "react";
import ace from "ace-builds";
import "ace-builds/src-noconflict/mode-html";
import "ace-builds/src-noconflict/theme-monokai";
import "ace-builds/src-noconflict/ext-language_tools";

interface AceHtmlEditorProps {
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  height?: string;
}

export function AceHtmlEditor({ value = "", onChange, placeholder, height = "320px" }: AceHtmlEditorProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const editorRef = useRef<ace.Editor | null>(null);

  useEffect(() => {
    if (!containerRef.current || editorRef.current) return;

    const editor = ace.edit(containerRef.current, {
      mode: "ace/mode/html",
      theme: "ace/theme/monokai",
      fontSize: 14,
      showPrintMargin: false,
      wrap: true,
      enableBasicAutocompletion: true,
      enableLiveAutocompletion: true,
      enableSnippets: true,
      value,
    });

    editor.setOptions({
      placeholder,
    });

    editor.on("change", () => {
      onChange?.(editor.getValue());
    });

    editorRef.current = editor;

    return () => {
      editor.destroy();
      editorRef.current = null;
    };
  }, []);

  useEffect(() => {
    if (editorRef.current && editorRef.current.getValue() !== value) {
      editorRef.current.setValue(value, -1);
    }
  }, [value]);

  return (
    <div
      ref={containerRef}
      style={{
        height,
        width: "100%",
        borderRadius: 8,
        border: "1px solid #d9d9d9",
        overflow: "hidden",
      }}
    />
  );
}
