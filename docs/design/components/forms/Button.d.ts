export interface ButtonProps {
  variant?: "primary" | "secondary" | "ghost" | "destructive";
  size?: "sm" | "md";
  children?: React.ReactNode;
  onClick?: () => void;
  style?: React.CSSProperties;
}
