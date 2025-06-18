import React, { useState } from "react";
import { Button } from "./Button";
import { AuthModals } from "./AuthModals";
import { useAuth } from "./AuthContext";
import { FiPlusCircle, FiUser, FiLogOut } from "react-icons/fi";
import { useToast } from "./ToastContext";
import { Link } from "react-router-dom";

export default function Header() {
  const [modalType, setModalType] = useState(null);
  const { user, logout } = useAuth();
  const { showToast } = useToast();

  const handleLogout = () => {
    logout();
    showToast("Uspješno ste se odjavili", "success");
  };

  return (
    <header className="w-full px-6 py-4 shadow-sm bg-white">
      <div className="max-w-6xl mx-auto flex justify-between items-center">
        <Link to="/" className="flex items-center gap-2">
          {/* Logo on the left */}
          <img src="./ikona.png" alt="GastroHub logo" className="h-8 w-auto" />
          <h1 className="text-xl font-semibold text-gray-900">GastroHub</h1>
        </Link>

        {user ? (
          <div className="flex gap-4 items-center">
            <Link to="/create-recipe">
              <Button className="flex items-center gap-2">
                <FiPlusCircle className="text-lg" />
                Objavi recept
              </Button>
            </Link>
            <Link to="/my-profile">
              <Button variant="outline" className="flex items-center gap-2">
                <FiUser className="text-lg" />
                Moj profil
              </Button>
            </Link>
            <Button onClick={handleLogout} className="flex items-center gap-2">
              <FiLogOut className="text-lg" />
              Odjava
            </Button>
          </div>
        ) : (
          <div className="flex gap-4">
            <Button variant="outline" onClick={() => setModalType("register")}>
              Registriraj se
            </Button>
            <Button onClick={() => setModalType("login")}>Prijavi se</Button>
          </div>
        )}
      </div>

      <AuthModals
        type={modalType}
        isOpen={modalType !== null}
        onClose={() => setModalType(null)}
      />
    </header>
  );
}
