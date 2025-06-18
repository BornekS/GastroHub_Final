// components/Dialog.jsx
import React from "react";
import { createPortal } from "react-dom";
import clsx from "clsx";

export function Dialog({ isOpen, onClose, children }) {
  if (!isOpen) return null;

  return createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div
        className="bg-white rounded-lg p-6 shadow-lg w-full max-w-md relative"
        onClick={(e) => e.stopPropagation()}
      >
        <button
          onClick={onClose}
          className="absolute top-2 right-3 text-gray-500 hover:text-black text-xl"
        >
          ×
        </button>
        {children}
      </div>
    </div>,
    document.body
  );
}
