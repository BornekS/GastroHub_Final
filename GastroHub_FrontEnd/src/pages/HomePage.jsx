import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Input } from "../components/Input";
import { Button } from "../components/Button";
import { Spinner } from "../components/Spinner";
import MultiRangeSlider from "../components/MultiRangeSlider";
import { useAuth } from "../components/AuthContext";
import RecipeCard from "../components/RecipeCard";

export default function HomePage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  /* form state */
  const [query, setQuery] = useState("");
  const [includeIngredients, setIncludeIngredients] = useState("");
  const [excludeIngredients, setExcludeIngredients] = useState("");
  const [prepTime, setPrepTime] = useState([10, 120]); // [min, max]
  const [categoryId, setCategoryId] = useState("");

  /* auxiliary */
  const [categories, setCategories] = useState([]);

  /* feed state */
  const [latestRecipes, setLatestRecipes] = useState([]);
  const [loading, setLoading] = useState(true);

  /* ───────────── Fetch categories (runs once) ───────────── */
  useEffect(() => {
    fetch("http://localhost:5292/api/Categories")
      .then((r) => r.json())
      .then(setCategories)
      .catch((e) => console.error("Failed to load categories:", e));
  }, []);

  /* ───────────── Najnoviji recepti ───────────── */
  useEffect(() => {
    setLoading(true);
    fetch(
      "http://localhost:5292/api/Recipes?pageNumber=1&pageSize=5&sortBy=publishedDateDesc"
    )
      .then((r) => r.json())
      .then(setLatestRecipes)
      .catch((e) => console.error("Failed to load latest recipes:", e))
      .finally(() => setLoading(false));
  }, []);

  /* ───────────── overlay likes ───────────── */
  useEffect(() => {
    if (!user) return;

    fetch("http://localhost:5292/api/Users/me", {
      headers: { Authorization: `Bearer ${user.token}` },
    })
      .then((r) => r.json())
      .then(({ likedRecipes }) =>
        setLatestRecipes((prev) =>
          prev.map((rec) => ({
            ...rec,
            likedByCurrentUser: likedRecipes.includes(rec.id),
          }))
        )
      )
      .catch((e) => console.error("Error fetching liked recipes:", e));
  }, [user]);

  /* ───────────── slider wrapper ───────────── */
  const handlePrepTimeChange = (value) => {
    if (Array.isArray(value)) {
      setPrepTime(value);
    } else if (value && typeof value === "object") {
      const min = value.min ?? value.minValue ?? prepTime[0];
      const max = value.max ?? value.maxValue ?? prepTime[1];
      setPrepTime([min, max]);
    }
  };

  /* ───────────── SEARCH SUBMIT ───────────── */
  const handleSearch = () => {
    const params = new URLSearchParams();

    if (query.trim()) params.append("query", query.trim());
    if (includeIngredients.trim())
      params.append("includeIngredients", includeIngredients.trim());
    if (excludeIngredients.trim())
      params.append("excludeIngredients", excludeIngredients.trim());
    if (categoryId) params.append("categoryId", categoryId);

    /* slider ALWAYS travels */
    params.append("minPrepTime", prepTime[0]);
    params.append("maxPrepTime", prepTime[1]);

    params.append("pageNumber", 1);
    params.append("pageSize", 10);

    navigate(`/search?${params.toString()}`);
  };

  /* ───────────── like / unlike ───────────── */
  const handleLike = (recipeId, isLiked) => {
    if (!user) return alert("You need to be logged in to like a recipe.");

    fetch(`http://localhost:5292/api/Likes/recipe/${recipeId}`, {
      method: isLiked ? "DELETE" : "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
      },
    })
      .then((r) =>
        r.status === 401 ? Promise.reject("Unauthorized") : r.json()
      )
      .then(() =>
        setLatestRecipes((prev) =>
          prev.map((rec) =>
            rec.id === recipeId
              ? {
                  ...rec,
                  likesCount: rec.likesCount + (isLiked ? -1 : 1),
                  likedByCurrentUser: !isLiked,
                }
              : rec
          )
        )
      )
      .catch((e) => console.error("Error liking recipe:", e));
  };

  /* ───────────── UI ───────────── */
  return (
    <div className="w-full">
      {/* Search banner */}
      <div className="relative w-full overflow-hidden mb-16">
        <div className="absolute inset-0 bg-cover bg-center opacity-90 bg-[url('pozadina-search.jpg')]" />
        <div className="relative z-10 py-24">
          <div className="max-w-4xl mx-auto p-6 bg-white/100 backdrop-blur-sm rounded-4xl shadow-lg">
            <h1 className="text-3xl font-bold mb-6 text-center pt-6 font-fancy">
              Što ćemo pripremiti danas?
            </h1>
            <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 pb-6">
              <Input
                label="Upit za pretragu"
                placeholder="Pretražite pojam..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
              />
              <Input
                label="Uključi sastojke"
                placeholder="Recept uključuje sastojke:"
                value={includeIngredients}
                onChange={(e) => setIncludeIngredients(e.target.value)}
              />
              <Input
                label="Isključi sastojke"
                placeholder="Recept ne uključuje sastojke:"
                value={excludeIngredients}
                onChange={(e) => setExcludeIngredients(e.target.value)}
              />

              {/* 🔽 Category dropdown */}
              <div>
                <label
                  htmlFor="categorySelect"
                  className="block text-sm font-medium mb-1 text-sm text-gray-700 font-medium"
                >
                  Kategorija jela
                </label>
                <select
                  id="categoryId"
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                  /* turn the whole <select> grey while the placeholder is showing */
                  className={`w-full border border-neutral-300 p-2 border rounded-md ${
                    !categoryId ? "text-neutral-400" : "text-neutral-900"
                  }`}
                >
                  {/* placeholder - disabled so the user can’t re-select it */}
                  <option value="" disabled className="text-gray-400">
                    Odaberite kategoriju
                  </option>

                  {categories.map((c) => (
                    <option
                      key={c.id}
                      value={c.id}
                      className="text-neutral-900"
                    >
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="col-span-1 sm:col-span-2">
                <label className="block text-sm font-medium mb-1">
                  Vrijeme pripreme (min)
                </label>
                <MultiRangeSlider
                  min={0}
                  max={180}
                  value={prepTime}
                  /* 🔑 use wrapper to normalise emitted value */
                  onChange={handlePrepTimeChange}
                />
              </div>
              <div className="col-span-1 sm:col-span-2 flex justify-center">
                <Button onClick={handleSearch}>Pretraživanje</Button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Najnoviji recepti */}
      <div className="max-w-4xl mx-auto px-6">
        <h2 className="text-2xl font-semibold mb-4">Najnoviji recepti</h2>

        {loading ? (
          <Spinner />
        ) : latestRecipes.length === 0 ? (
          <p className="text-gray-500 text-center p-12">
            Još nema objavljenih recepata.
          </p>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {latestRecipes.map((r) => (
              <RecipeCard
                key={r.id}
                recipe={r}
                onLike={handleLike}
                enableFavorite={true}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
