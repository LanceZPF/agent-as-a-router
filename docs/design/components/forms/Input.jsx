import React from "react";

export function Input({ type = "text", placeholder, value, onChange, style, ...rest }) {
  return (
    <input
      type={type}
      placeholder={placeholder}
      value={value}
      onChange={onChange}
      style={{
        background: "var(--surface-inset)",
        border: "1px solid var(--border-default)",
        color: "var(--text-primary)",
        outline: "none",
        borderRadius: "var(--radius-md)",
        padding: "6px 10px",
        fontFamily: "var(--font-sans)",
        fontSize: 13,
        transition: "border-color var(--duration-fast) ease",
        ...style,
      }}
      onFocus={(e) => (e.currentTarget.style.borderColor = "var(--focus-ring)")}
      onBlur={(e) => (e.currentTarget.style.borderColor = "var(--border-default)")}
      {...rest}
    />
  );
}
