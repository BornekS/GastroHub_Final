import React from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import HomePage from "./pages/HomePage";
import RecipePage from "./pages/RecipePage";
import CreateRecipePage from "./pages/CreateRecipePage";
import EditRecipePage from "./pages/EditRecipePage";
import SearchResultsPage from "./pages/SearchResultsPage";
import MyProfilePage from "./pages/MyProfilePage";

import Layout from "./components/Layout";
import { useAuth } from "./components/AuthContext";
import "./index.css";
import { ToastProvider } from "./components/ToastContext";
import { AuthProvider } from "./components/AuthContext";

const RequireAuth = ({ children }) => {
  const { user } = useAuth();
  return user ? children : <Navigate to="/" />;
};

createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <ToastProvider>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Layout />}>
              <Route index element={<HomePage />} />
              <Route path="search" element={<SearchResultsPage />} />
              <Route path="recipe/:id" element={<RecipePage />} />

              <Route
                path="create-recipe"
                element={
                  <RequireAuth>
                    <CreateRecipePage />
                  </RequireAuth>
                }
              />
              <Route
                path="edit-recipe/:id"
                element={
                  <RequireAuth>
                    <EditRecipePage />
                  </RequireAuth>
                }
              />
              <Route
                path="my-profile"
                element={
                  <RequireAuth>
                    <MyProfilePage />
                  </RequireAuth>
                }
              />
            </Route>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ToastProvider>
  </React.StrictMode>
);
