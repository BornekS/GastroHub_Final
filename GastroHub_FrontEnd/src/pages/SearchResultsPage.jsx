import React, { useEffect, useState, useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Spinner } from "../components/Spinner";
import { useAuth } from "../components/AuthContext";
import RecipeCard from "../components/RecipeCard";
import { FaArrowLeft } from "react-icons/fa";

export default function SearchResultsPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { search } = useLocation();

  /* recipes */
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(true);

  /* human-readable filter chips */
  const params = useMemo(() => new URLSearchParams(search), [search]);
  const query = params.get("query")?.trim();
  const include = params.get("includeIngredients")?.trim();
  const exclude = params.get("excludeIngredients")?.trim();
  const categoryId = params.get("categoryId")?.trim();
  const minTime = params.get("minPrepTime");
  const maxTime = params.get("maxPrepTime");

  /* category map for id → name */
  const [catName, setCatName] = useState("");
  useEffect(() => {
    if (!categoryId) return setCatName("");
    fetch("http://localhost:5292/api/Categories")
      .then((r) => (r.ok ? r.json() : []))
      .then((cats) =>
        setCatName(
          cats.find((c) => String(c.id) === categoryId)?.name || categoryId
        )
      )
      .catch(() => setCatName(categoryId));
  }, [categoryId]);

  /* chip list */
  const chips = useMemo(() => {
    const out = [];
    if (query) out.push({ label: "Upit", value: query });
    if (include) out.push({ label: "Uključi", value: include });
    if (exclude) out.push({ label: "Isključi", value: exclude });
    if (categoryId)
      out.push({ label: "Kategorija", value: catName || categoryId });
    if (minTime || maxTime)
      out.push({
        label: "Vrijeme",
        value: `${minTime || 0}–${maxTime || "∞"} min`,
      });
    return out;
  }, [query, include, exclude, categoryId, catName, minTime, maxTime]);

  /* —— fetch results whenever URL changes —— */
  useEffect(() => {
    setLoading(true);
    fetch(`http://localhost:5292/api/Recipes${search}`)
      .then((r) => r.json())
      .then(setRecipes)
      .catch((e) => console.error("Failed to load recipes:", e))
      .finally(() => setLoading(false));
  }, [search]);

  /* —— like / unlike —— */
  const handleLike = (recipeId, isLiked) => {
    if (!user) return alert("You need to be logged in to like a recipe.");

    setRecipes((prev) =>
      prev.map((rec) =>
        rec.id === recipeId
          ? {
              ...rec,
              likedByCurrentUser: !isLiked,
              likesCount: rec.likesCount + (isLiked ? -1 : 1),
            }
          : rec
      )
    );

    fetch(`http://localhost:5292/api/Likes/recipe/${recipeId}`, {
      method: isLiked ? "DELETE" : "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
      },
    }).catch((e) => console.error("Error liking recipe:", e));
  };

  /* —— render —— */
  return (
    <div className="max-w-6xl mx-auto px-6 py-10">
      {/* back arrow */}
      <button
        onClick={() => navigate(-1)}
        className="mb-4 flex items-center text-grey-600 hover:text-orange-600 cursor-pointer transition focus:outline-none"
      >
        <FaArrowLeft className="mr-2" />
        <span className="sr-only sm:not-sr-only sm:inline">Natrag</span>
      </button>

      <h1 className="text-3xl font-bold mb-6">Rezultati pretraživanja</h1>

      {/* filter chips */}
      {chips.length > 0 && (
        <div className="mb-8 flex flex-wrap gap-3">
          {chips.map((c) => (
            <span
              key={c.label}
              className="bg-orange-50 text-orange-600 border border-orange-200 px-3 py-1 rounded-full text-sm"
            >
              {c.label}: <span className="font-medium">{c.value}</span>
            </span>
          ))}
        </div>
      )}

      {loading ? (
        <Spinner />
      ) : recipes.length === 0 ? (
        <p className="text-gray-500 text-center p-12">
          Nema recepata za zadane kriterije.
        </p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {recipes.map((recipe) => (
            <RecipeCard
              key={recipe.id}
              recipe={recipe}
              onLike={handleLike}
              enableFavorite={true}
            />
          ))}
        </div>
      )}
    </div>
  );
}
