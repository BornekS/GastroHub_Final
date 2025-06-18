import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  FaClock,
  FaUserAlt,
  FaHeart,
  FaRegHeart,
  FaArrowLeft,
} from "react-icons/fa";
import { useAuth } from "../components/AuthContext";
import { Spinner } from "../components/Spinner";
import RecipeCard from "../components/RecipeCard"; // 🆕 prikaz dodatnih recepata

import dayjs from "dayjs";
import utc from "dayjs/plugin/utc";
import "dayjs/locale/hr";

dayjs.extend(utc);
dayjs.locale("hr");

function formatDate(raw) {
  if (!raw) return "—";
  const safe = raw.replace(/(\.\d{3})\d+$/, "$1") + "Z";
  const d = dayjs.utc(safe);
  return d.isValid() ? d.local().format("D. MMM YYYY HH:mm") : "—";
}

export default function RecipePage() {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();

  /* ───────────── main recipe ───────────── */
  const [recipe, setRecipe] = useState(null);
  const [loadingRecipe, setLoadingRecipe] = useState(true);

  /* ───────────── recipe like ───────────── */
  const [likesCount, setLikesCount] = useState(0);
  const [liked, setLiked] = useState(false);
  const [likePending, setLikePending] = useState(false);

  /* ───────────── comments ───────────── */
  const [comments, setComments] = useState([]);
  const [loadingComments, setLoadingComments] = useState(true);
  const [newCommentText, setNewCommentText] = useState("");

  /* 🆕  ostali recepti autora */
  const [otherRecipes, setOtherRecipes] = useState([]);

  const [loadingOther, setLoadingOther] = useState(false);

  /* ──────────────────────────────────────────
   *  FETCH: recipe + comments
   * ────────────────────────────────────────── */
  useEffect(() => {
    let isMounted = true;

    /* recipe */
    fetch(`http://localhost:5292/api/Recipes/${id}`, {
      headers: user ? { Authorization: `Bearer ${user.token}` } : {},
    })
      .then((res) => res.json())
      .then((data) => {
        if (!isMounted) return;
        setRecipe(data);
        setLikesCount(data.likesCount ?? 0);
        setLiked(Boolean(data.likedByCurrentUser));
        setLoadingRecipe(false);
      })
      .catch((err) => {
        console.error("Error fetching recipe:", err);
        if (isMounted) setLoadingRecipe(false);
      });

    /* comments */
    fetch(`http://localhost:5292/api/Comments/recipe/${id}`)
      .then((res) => res.json())
      .then((data) => {
        if (!isMounted) return;
        setComments(data);
        setLoadingComments(false);
      })
      .catch((err) => {
        console.error("Error fetching comments:", err);
        if (isMounted) setLoadingComments(false);
      });

    return () => {
      isMounted = false;
    };
  }, [id, user]);

  /* ──────────────────────────────────────────
   *  FETCH: other recipes by author
   * ────────────────────────────────────────── */
  useEffect(() => {
    /* prihvati oba oblika: userId (camel) ili UserId (pascal) */
    const authorId = recipe?.userId ?? recipe?.UserId;
    if (!authorId) return;

    setLoadingOther(true);

    fetch(`http://localhost:5292/api/Recipes/user/${authorId}`)
      .then((res) => {
        if (!res.ok) throw new Error(res.status);
        return res.json();
      })
      .then((data) => setOtherRecipes(data.filter((r) => r.id !== recipe.id)))
      .catch((err) => console.error("Error fetching author's recipes:", err))
      .finally(() => setLoadingOther(false));
  }, [recipe?.userId, recipe?.UserId, recipe?.id]);

  /* ──────────────────────────────────────────
   *  LIKE / UNLIKE main recipe
   * ────────────────────────────────────────── */
  const toggleRecipeLike = () => {
    if (!user) {
      alert("Morate biti prijavljeni da biste lajkali recept.");
      return;
    }
    if (likePending) return;

    const next = !liked;
    setLiked(next);
    setLikesCount((c) => c + (next ? 1 : -1));
    setLikePending(true);

    fetch(`http://localhost:5292/api/Likes/recipe/${id}`, {
      method: next ? "POST" : "DELETE",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
      },
    })
      .catch((e) => {
        console.error(e);
        setLiked((l) => !l);
        setLikesCount((c) => c + (next ? -1 : +1));
      })
      .finally(() => setLikePending(false));
  };

  /* 🆕 LIKE from other-recipes list */
  const handleCardLike = (recipeId, currentlyLiked) => {
    if (!user) {
      alert("Morate biti prijavljeni da biste lajkali recept.");
      return;
    }
    const next = !currentlyLiked;

    setOtherRecipes((prev) =>
      prev.map((r) =>
        r.id === recipeId
          ? {
              ...r,
              likedByCurrentUser: next,
              likesCount: r.likesCount + (next ? 1 : -1),
            }
          : r
      )
    );

    fetch(`http://localhost:5292/api/Likes/recipe/${recipeId}`, {
      method: next ? "POST" : "DELETE",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
      },
    }).catch((err) => {
      console.error(err);
      setOtherRecipes((prev) =>
        prev.map((r) =>
          r.id === recipeId
            ? {
                ...r,
                likedByCurrentUser: currentlyLiked,
                likesCount: r.likesCount + (next ? -1 : +1),
              }
            : r
        )
      );
    });
  };

  /* ──────────────────────────────────────────
   *  COMMENTS helpers
   * ────────────────────────────────────────── */
  const updateCommentRecursive = (comment, id, fn) => {
    if (comment.id === id) return fn(comment);
    if (comment.replies)
      return {
        ...comment,
        replies: comment.replies.map((r) => updateCommentRecursive(r, id, fn)),
      };
    return comment;
  };

  const handleCommentLikeToggle = (commentId, currentlyLiked) => {
    if (!user) {
      alert("Morate biti prijavljeni da biste lajkali komentar.");
      return;
    }
    const next = !currentlyLiked;

    setComments((prev) =>
      prev.map((c) =>
        updateCommentRecursive(c, commentId, (com) => ({
          ...com,
          likedByCurrentUser: next,
          likesCount: com.likesCount + (next ? 1 : -1),
        }))
      )
    );

    fetch(`http://localhost:5292/api/Likes/comment/${commentId}`, {
      method: next ? "POST" : "DELETE",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
      },
    }).catch((err) => {
      console.error(err);
      /* rollback */
      setComments((prev) =>
        prev.map((c) =>
          updateCommentRecursive(c, commentId, (com) => ({
            ...com,
            likedByCurrentUser: currentlyLiked,
            likesCount: com.likesCount + (next ? -1 : +1),
          }))
        )
      );
    });
  };

  const handlePostComment = () => {
    if (!user) {
      alert("Morate biti prijavljeni da biste komentirali.");
      return;
    }
    if (!newCommentText.trim()) return;

    const tempId = Date.now();
    const optimistic = {
      id: tempId,
      content: newCommentText,
      userDisplayName: user.username ?? "Vi",
      createdAt: new Date().toISOString(),
      likesCount: 0,
      likedByCurrentUser: false,
      replies: [],
    };
    setComments((prev) => [optimistic, ...prev]);
    setNewCommentText("");

    fetch(`http://localhost:5292/api/Comments/recipe/${id}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.token}`,
      },
      body: JSON.stringify({ content: newCommentText }),
    })
      .then((res) => res.json())
      .then((data) =>
        setComments((prev) => prev.map((c) => (c.id === tempId ? data : c)))
      )
      .catch((err) => {
        console.error(err);
        setComments((prev) => prev.filter((c) => c.id !== tempId));
      });
  };

  const splitLines = (txt) =>
    txt
      .split(/\r?\n|;/)
      .map((s) => s.trim())
      .filter(Boolean);

  /* ──────────────────────────────────────────
   *  RENDER
   * ────────────────────────────────────────── */
  if (loadingRecipe)
    return (
      <div className="flex h-[60vh] items-center justify-center">
        <Spinner />
      </div>
    );

  if (!recipe)
    return (
      <div className="flex h-[60vh] items-center justify-center text-lg font-semibold text-gray-600">
        Recept nije pronađen.
      </div>
    );

  return (
    <div className="mx-auto max-w-7xl p-4 md:p-8">
      {/* ← back */}
      <button
        onClick={() => navigate(-1)}
        className="my-6 flex items-center text-grey-600 hover:text-orange-600 transition"
      >
        <FaArrowLeft className="mr-2" />
        <span className="sr-only sm:not-sr-only sm:inline cursor-pointer">
          Natrag
        </span>
      </button>

      {/* cover */}
      {recipe.imageUrls?.[0] && (
        <img
          src={`http://localhost:5292${recipe.imageUrls[0]}`}
          alt={recipe.name}
          className="h-80 w-full rounded-2xl object-cover shadow-md md:h-[28rem]"
        />
      )}

      {/* title */}
      <h1 className="mt-6 text-center text-4xl font-extrabold md:text-5xl">
        {recipe.name}
      </h1>

      {/* meta */}
      <div className="mt-4 flex flex-wrap items-center justify-center gap-8 text-base text-gray-500">
        <div className="flex items-center gap-2">
          <FaUserAlt className="text-2xl" />
          <span>{recipe.userDisplayName}</span>
        </div>
        <div className="flex items-center gap-2">
          <FaClock className="text-2xl" />
          <span>{recipe.preparationTimeMinutes} min</span>
        </div>
        <span className="rounded-full bg-amber-100 px-4 py-1.5 font-medium text-amber-800">
          {recipe.categoryName}
        </span>
      </div>

      {/* like main recipe */}
      <div className="mt-6 flex justify-center">
        <button
          onClick={toggleRecipeLike}
          disabled={likePending}
          className="flex items-center gap-2 rounded-full border border-orange-300 px-5 py-2 transition hover:bg-orange-50 disabled:opacity-50"
        >
          {liked ? (
            <FaHeart className="text-orange-600" />
          ) : (
            <FaRegHeart className="text-orange-600" />
          )}
          <span>{likesCount}</span>
        </button>
      </div>

      {/* ingredients + instructions */}
      <section className="mt-12 grid gap-12 md:grid-cols-2">
        <div>
          <h2 className="mb-4 text-2xl font-semibold">Sastojci</h2>
          <ul className="list-disc list-inside space-y-2 text-gray-700">
            {splitLines(recipe.ingredients).map((ing, i) => (
              <li key={i}>{ing}</li>
            ))}
          </ul>
        </div>
        <div>
          <h2 className="mb-4 text-2xl font-semibold">Upute</h2>
          <ol className="list-decimal list-inside space-y-3 text-gray-700">
            {splitLines(recipe.instructions).map((s, i) => (
              <li key={i}>{s}</li>
            ))}
          </ol>
        </div>
      </section>

      {/* 🆕 other recipes by author */}

      {/* comments */}
      <section className="mt-16">
        <h2 className="mb-6 text-2xl font-semibold">Komentari</h2>

        {/* new comment */}
        <div className="mb-8">
          <textarea
            className="w-full resize-none rounded-lg border border-gray-400 p-3 focus:border-orange-400 focus:outline-none"
            rows={3}
            placeholder={
              user ? "Objavi komentar…" : "Prijavite se za komentiranje…"
            }
            value={newCommentText}
            disabled={!user}
            onChange={(e) => setNewCommentText(e.target.value)}
          />
          <div className="mt-2 text-right">
            <button
              onClick={handlePostComment}
              disabled={!user || !newCommentText.trim()}
              className="rounded-lg bg-orange-500 px-5 py-2 text-white transition disabled:opacity-50"
            >
              Objavi
            </button>
          </div>
        </div>

        {/* list */}
        {loadingComments ? (
          <Spinner />
        ) : comments.length === 0 ? (
          <p className="text-gray-500">
            Još nema komentara. Budite prvi koji će podijeliti svoje mišljenje!
          </p>
        ) : (
          <div className="space-y-6">
            {comments.map((c) => (
              <CommentItem
                key={c.id}
                comment={c}
                depth={0}
                onToggleLike={handleCommentLikeToggle}
              />
            ))}
          </div>
        )}
      </section>

      <section className="mt-16">
        <h2 className="mb-6 text-2xl font-semibold">
          Ostali recepti ovog autora
        </h2>

        {loadingOther ? (
          <Spinner />
        ) : otherRecipes.length === 0 ? (
          <p className="text-gray-500">
            Autor nema drugih objavljenih recepata.
          </p>
        ) : (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {otherRecipes.map((r) => (
              <RecipeCard
                key={r.id}
                recipe={r}
                onLike={() => handleCardLike(r.id, r.likedByCurrentUser)}
                enableFavorite
              />
            ))}
          </div>
        )}
      </section>
    </div>
  );

  /* ────────── CommentItem ────────── */
  function CommentItem({ comment, depth, onToggleLike }) {
    return (
      <div className="flex" style={{ marginLeft: depth * 24 }}>
        {/* vote */}
        <div className="mr-3 flex flex-col items-center text-sm text-gray-400">
          <button
            onClick={() => onToggleLike(comment.id, comment.likedByCurrentUser)}
            className="hover:text-orange-600"
          >
            {comment.likedByCurrentUser ? <FaHeart /> : <FaRegHeart />}
          </button>
          <span className="text-xs text-gray-600">{comment.likesCount}</span>
        </div>

        {/* bubble */}
        <div className="flex-1 rounded-lg border border-gray-400 bg-white p-3">
          <div className="mb-1 flex items-center text-xs text-gray-500">
            <span className="font-medium text-gray-700">
              {comment.userDisplayName}
            </span>
            <span className="mx-2">•</span>
            <span>{formatDate(comment.createdAt)}</span>
          </div>
          <p className="whitespace-pre-line text-sm text-gray-800">
            {comment.content}
          </p>
        </div>
      </div>
    );
  }
}
