import React from "react";
import clsx from "clsx";

export function Button({
  children,
  className = "",
  variant = "default",
  ...props
}) {
  const baseStyles = "px-4 py-2 rounded-md transition-colors cursor-pointer";

  const variants = {
    default: "bg-orange-600 text-white hover:bg-orange-700",
    outline: "border border-gray-300 text-gray-800 bg-white hover:bg-gray-100",
    ghost: "text-orange-600 hover:bg-orange-50",
  };

  return (
    <button
      className={clsx(baseStyles, variants[variant], className)}
      {...props}
    >
      {children}
    </button>
  );
}
