export interface TabsProps {
  /** Ordered tab list. */
  tabs: { id: string; label: string }[];
  /** Currently active tab id. */
  active: string;
  onChange?: (id: string) => void;
}
