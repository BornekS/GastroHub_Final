// components/Spinner.jsx
import React from "react";

export function Spinner() {
  return (
    <div className="flex justify-center items-center py-8">
      <div className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin" />
    </div>
  );
}
