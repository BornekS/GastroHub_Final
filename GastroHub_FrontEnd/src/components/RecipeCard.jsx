import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { FaClock, FaUserAlt, FaHeart, FaRegStar, FaStar } from "react-icons/fa";

export default function RecipeCard({ recipe, onLike, className = "" }) {
  if (!recipe) return null;

  const {
    id,
    imageUrls,
    name,
    userDisplayName,
    preparationTimeMinutes,
    categoryName,
    likesCount: initialLikes,
    likedByCurrentUser: initiallyLiked,
  } = recipe;

  /* ───────────── local LIKE state (for snappier UI) ───────────── */
  const [liked, setLiked] = useState(initiallyLiked);
  const [likes, setLikes] = useState(initialLikes);

  /* keep local ↔ prop in sync when parent list refetches */
  useEffect(() => {
    setLiked(initiallyLiked);
    setLikes(initialLikes);
  }, [initiallyLiked, initialLikes]);

  const [animate, setAnimate] = useState(false);
  useEffect(() => {
    setAnimate(true);
    const t = setTimeout(() => setAnimate(false), 300);
    return () => clearTimeout(t);
  }, [liked]);

  const handleHeartClick = () => {
    const nextLiked = !liked;
    /* optimistic local update */
    setLiked(nextLiked);
    setLikes((prev) => prev + (nextLiked ? 1 : -1));
    onLike?.(id, liked);
  };

  return (
    <div
      className={`bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition ${className}`}
    >
      {/* —— Image —— */}
      <Link to={`/recipe/${id}`} className="block">
        <img
          src={`http://localhost:5292${imageUrls?.[0] ?? ""}`}
          alt={name}
          className="w-full h-48 object-cover"
        />
      </Link>

      {/* —— Card Body —— */}
      <div className="p-4">
        <Link to={`/recipe/${id}`} className="block hover:underline">
          <h3 className="text-lg font-semibold line-clamp-2">{name}</h3>
        </Link>

        {/* author */}
        <p className="text-sm text-gray-500 mb-2 flex items-center">
          <FaUserAlt className="mr-2" />
          {userDisplayName}
        </p>

        {/* meta row */}
        <div className="mt-4 flex justify-between items-center text-sm text-gray-500">
          <div className="flex items-center space-x-2">
            <FaClock className="text-gray-600" />
            <span>{preparationTimeMinutes} min</span>
          </div>
          <p>{categoryName}</p>
        </div>

        {/* actions row */}
        <div className="mt-4 flex justify-between items-center">
          <div className="flex items-center space-x-4">
            {/* yellow‑star “favorite” indicator */}

            {/* red heart for “like” */}
            <button
              type="button"
              className="flex items-center space-x-1 focus:outline-none select-none"
              onClick={handleHeartClick}
            >
              <FaHeart
                size={16}
                className={`transition-transform duration-300 ${
                  liked ? "text-red-500" : "text-gray-400"
                } ${animate ? "scale-125" : ""}`}
              />
              <span className="text-sm">{likes}</span>
            </button>
          </div>

          {liked && (
            <span className="text-blue-500 text-xs font-medium">
              Lajkali ste ovaj recept
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
