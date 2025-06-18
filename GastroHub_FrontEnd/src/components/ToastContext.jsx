import React, { createContext, useContext, useState } from "react";
import { createPortal } from "react-dom";
import clsx from "clsx";
import { FiCheckCircle, FiAlertTriangle, FiXCircle, FiX } from "react-icons/fi";

const ToastContext = createContext();

export function useToast() {
  return useContext(ToastContext);
}

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const showToast = (message, variant = "success") => {
    const id = Date.now();
    setToasts((prev) => [...prev, { id, message, variant }]);
    setTimeout(() => removeToast(id), 4000);
  };

  const removeToast = (id) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  };

  const getIcon = (variant) => {
    const baseClass = "text-2xl mr-3";
    switch (variant) {
      case "success":
        return <FiCheckCircle className={clsx(baseClass, "text-green-600")} />;
      case "error":
        return <FiXCircle className={clsx(baseClass, "text-red-600")} />;
      case "warning":
        return (
          <FiAlertTriangle className={clsx(baseClass, "text-yellow-500")} />
        );
      default:
        return null;
    }
  };

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      {createPortal(
        <div className="fixed bottom-4 left-4 space-y-3 z-[9999]">
          {toasts.map((toast) => (
            <div
              key={toast.id}
              className="px-5 py-4 rounded-xl shadow-lg bg-white flex items-center justify-between min-w-[280px] max-w-sm"
            >
              <div className="flex items-center text-base text-gray-800 font-medium">
                {getIcon(toast.variant)}
                <span>{toast.message}</span>
              </div>
              <button
                onClick={() => removeToast(toast.id)}
                className="ml-4 text-gray-400 hover:text-gray-600 text-xl leading-none"
              >
                <FiX />
              </button>
            </div>
          ))}
        </div>,
        document.body
      )}
    </ToastContext.Provider>
  );
}
