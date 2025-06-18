import React from "react";
import clsx from "clsx";

export function Input({ label, className = "", id, ...props }) {
  const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;

  return (
    <div className="flex flex-col gap-1">
      {label && (
        <label htmlFor={inputId} className="text-sm text-gray-700 font-medium">
          {label}
        </label>
      )}
      <input
        id={inputId}
        className={clsx(
          "w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500",
          className
        )}
        {...props}
      />
    </div>
  );
}
