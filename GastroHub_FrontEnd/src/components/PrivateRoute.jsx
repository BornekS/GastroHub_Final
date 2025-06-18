import React from "react";
import { Navigate, Route } from "react-router-dom";
import { useAuth } from "../components/AuthContext"; // Custom hook to check for authentication

// PrivateRoute for React Router v6
const PrivateRoute = ({ element, ...rest }) => {
  const { user } = useAuth(); // Check if the user is logged in

  return user ? element : <Navigate to="/" replace />; // Redirect to login if not authenticated
};

export default PrivateRoute;
