import React, { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { FaArrowLeft, FaTrashAlt, FaEdit } from "react-icons/fa";
import { useToast } from "../components/ToastContext";
import RecipeCard from "../components/RecipeCard";

/**
 * Helper that removes duplicate objects by `id` while preserving order.
 */
const uniqueById = (arr = []) => {
  const seen = new Set();
  return arr.filter((item) => {
    if (seen.has(item.id)) return false;
    seen.add(item.id);
    return true;
  });
};

export default function MyProfilePage() {
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [myRecipes, setMyRecipes] = useState([]);
  const [likedRecipes, setLikedRecipes] = useState([]);
  const [loadingMy, setLoadingMy] = useState(true);
  const [loadingLiked, setLoadingLiked] = useState(true);
  const [error, setError] = useState("");

  /* confirm delete dialog */
  const [confirmDeleteId, setConfirmDeleteId] = useState(null);

  /* ───────────── Auth helpers ───────────── */
  const authUser = JSON.parse(localStorage.getItem("authUser"));
  const token = authUser?.token;
  const userId = authUser?.id;

  /* ───────────── Like / Unlike handler ───────────── */
  const handleLikeToggle = async (recipeId, currentlyLiked) => {
    try {
      const method = currentlyLiked ? "DELETE" : "POST";
      const res = await fetch(
        `http://localhost:5292/api/Likes/recipe/${recipeId}`,
        {
          method,
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      if (!res.ok) throw new Error("Greška pri ažuriranju lajka");
    } catch (err) {
      showToast(err.message, "error");
    }
  };

  /* ───────────── Edit & Delete handlers ───────────── */
  const handleEditRecipe = (id) => navigate(`/edit-recipe/${id}`);

  const handleDeleteRecipe = (id) => {
    setConfirmDeleteId(id);
  };

  const executeDelete = async (id) => {
    try {
      const res = await fetch(`http://localhost:5292/api/Recipes/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Brisanje nije uspjelo");
      setMyRecipes((prev) => prev.filter((r) => r.id !== id));
      showToast("Recept izbrisan", "success");
    } catch (err) {
      showToast(err.message, "error");
    } finally {
      setConfirmDeleteId(null);
    }
  };

  /* ───────────── Fetch data ───────────── */
  useEffect(() => {
    if (!token) return;

    /* ————— 1. Moji recepti ————— */
    const fetchMyRecipes = async () => {
      try {
        const res = await fetch("http://localhost:5292/api/Recipes", {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error("Neuspjelo učitavanje recepata");
        const all = await res.json();
        const mine = all.filter((r) => r.userId === userId);
        setMyRecipes(uniqueById(mine));
      } catch (err) {
        setError(err.message);
        showToast(err.message, "error");
      } finally {
        setLoadingMy(false);
      }
    };

    /* ————— 2. Recepti koje sam lajkao/la ————— */
    const fetchLikedRecipes = async () => {
      try {
        const res = await fetch("http://localhost:5292/api/Users/me", {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error("Neuspjelo učitavanje podataka");
        const userData = await res.json();

        const likedIds = userData.likedRecipes || userData.likedRecipeIds || [];

        if (likedIds.length) {
          const liked = await Promise.all(
            likedIds.map((id) =>
              fetch(`http://localhost:5292/api/Recipes/${id}`, {
                headers: { Authorization: `Bearer ${token}` },
              }).then((r) => (r.ok ? r.json() : null))
            )
          );
          setLikedRecipes(uniqueById(liked.filter(Boolean)));
        } else {
          setLikedRecipes([]);
        }
      } catch (err) {
        setError(err.message);
        showToast(err.message, "error");
      } finally {
        setLoadingLiked(false);
      }
    };

    fetchMyRecipes();
    fetchLikedRecipes();
  }, [token, userId]);

  /* ───────────── Render helpers ───────────── */
  const RecipesSection = ({ title, recipes, loading, editable }) => {
    const deduped = useMemo(() => uniqueById(recipes), [recipes]);

    return (
      <section className={title === "Moji recepti" ? "mb-12" : ""}>
        <h3 className="text-2xl font-semibold mb-4">{title}</h3>
        {loading ? (
          <p>Učitavanje...</p>
        ) : deduped.length ? (
          <div className="grid gap-6 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
            {deduped.map((r) => (
              <div key={r.id} className="relative group">
                {/* Action buttons only for owner */}
                {editable && (
                  <div className="absolute top-2 right-2 flex flex-col gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button
                      type="button"
                      onClick={() => handleEditRecipe(r.id)}
                      className="bg-white p-1 rounded-full shadow hover:bg-gray-100 focus:outline-none"
                    >
                      <FaEdit className="text-gray-600" />
                    </button>
                    <button
                      type="button"
                      onClick={() => handleDeleteRecipe(r.id)}
                      className="bg-white p-1 rounded-full shadow hover:bg-gray-100 focus:outline-none"
                    >
                      <FaTrashAlt className="text-red-500" />
                    </button>
                  </div>
                )}
                <RecipeCard recipe={r} onLike={handleLikeToggle} />
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500">Nema recepata za prikaz.</p>
        )}
      </section>
    );
  };

  /* ───────────── JSX ───────────── */
  return (
    <div className="mx-auto p-6 max-w-6xl relative">
      {/* Back button */}
      <button
        onClick={() => navigate(-1)}
        className="my-6 flex items-center text-grey-600 hover:text-orange-600 cursor-pointer transition focus:outline-none"
      >
        <FaArrowLeft className="mr-2" />
        <span className="sr-only sm:not-sr-only sm:inline">Natrag</span>
      </button>

      <h2 className="text-3xl font-bold mb-8">Moj profil</h2>

      {/* Sections */}
      <RecipesSection
        title="Moji recepti"
        recipes={myRecipes}
        loading={loadingMy}
        editable
      />

      <RecipesSection
        title="Recepti koje sam lajkao/la"
        recipes={likedRecipes}
        loading={loadingLiked}
        editable={false}
      />

      {error && <p className="text-red-500 mt-8 text-center">{error}</p>}

      {/* ───────────── Delete confirmation modal ───────────── */}
      {confirmDeleteId !== null && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-xl shadow-lg p-6 w-[90%] max-w-md">
            <h4 className="text-lg font-semibold mb-4">Potvrda brisanja</h4>
            <p className="mb-6 text-gray-600">
              Jeste li sigurni da želite izbrisati ovaj recept?
            </p>
            <div className="flex justify-end gap-3">
              <button
                type="button"
                className="px-4 py-2 rounded-md bg-gray-100 hover:bg-gray-200"
                onClick={() => setConfirmDeleteId(null)}
              >
                Odustani
              </button>
              <button
                type="button"
                className="px-4 py-2 rounded-md bg-red-500 text-white hover:bg-red-600"
                onClick={() => executeDelete(confirmDeleteId)}
              >
                Izbriši
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
