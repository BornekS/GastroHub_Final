import React, { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useDropzone } from "react-dropzone";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import { FaArrowLeft } from "react-icons/fa";
import { useToast } from "../components/ToastContext";

const validationSchema = Yup.object().shape({
  name: Yup.string().required("Naziv je obavezan"),
  ingredients: Yup.string().required("Sastojci su obavezni"),
  instructions: Yup.string().required("Upute su obavezne"),
  preparationTime: Yup.number()
    .typeError("Vrijeme mora biti broj")
    .positive("Mora biti pozitivno")
    .integer("Mora biti cijeli broj")
    .required("Vrijeme pripreme je obavezno"),
  categoryId: Yup.number()
    .typeError("Kategorija je obavezna")
    .moreThan(0, "Odaberite kategoriju")
    .required("Kategorija je obavezna"),
  image: Yup.mixed().required("Slika je obavezna"),
});

export default function CreateRecipePage() {
  const navigate = useNavigate();
  const { showToast } = useToast(); // 📢 2️⃣ grab toast helper

  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  /* ───────────── Fetch categories ───────────── */
  useEffect(() => {
    fetch("http://localhost:5292/api/Categories")
      .then((r) => (r.ok ? r.json() : Promise.reject()))
      .then(setCategories)
      .catch(() => {
        const msg = "Nije moguće učitati kategorije.";
        setError(msg);
        showToast(msg, "error"); // 📢 3️⃣ toast on fetch failure in HR
      });
  }, [showToast]);

  /* ───────────── Dropzone helper (memoised) ───────────── */
  const makeDropzone = useCallback(
    (setFieldValue) =>
      useDropzone({
        accept: { "image/*": [] },
        multiple: false,
        onDrop: (acceptedFiles) => {
          if (acceptedFiles[0]) {
            setFieldValue("image", acceptedFiles[0]);
          }
        },
      }),
    []
  );

  /* ───────────── Formik submit ───────────── */
  const handleSubmit = async (values, { resetForm }) => {
    setLoading(true);
    setError("");

    const authUser = JSON.parse(localStorage.getItem("authUser"));
    const token = authUser ? authUser.token : null;

    try {
      /* 1️⃣ upload the image first */
      const imageForm = new FormData();
      imageForm.append("file", values.image);

      const imgRes = await fetch("http://localhost:5292/api/Media/upload", {
        method: "POST",
        body: imageForm,
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!imgRes.ok) throw new Error("Neuspjelo učitavanje slike");
      const { url: imageUrl } = await imgRes.json();

      /* 2️⃣ send the recipe */
      const payload = {
        name: values.name,
        ingredients: values.ingredients,
        instructions: values.instructions,
        preparationTimeMinutes: parseInt(values.preparationTime, 10),
        categoryId: parseInt(values.categoryId, 10),
        imageUrl,
      };

      const recRes = await fetch("http://localhost:5292/api/Recipes", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });
      if (!recRes.ok) throw new Error("Neuspjelo objavljivanje recepta");

      /* ☑️ success: clear + navigate + toast */
      resetForm();
      showToast("Recept je uspješno objavljen!", "success"); // 📢 4️⃣ Croatian success message
      navigate("/");
    } catch (err) {
      const msg = err.message || "Greška pri spremanju recepta.";
      setError(msg);
      showToast(msg, "error"); // 📢 5️⃣ Croatian error toast
    } finally {
      setLoading(false);
    }
  };

  /* ───────────── UI ───────────── */
  return (
    <div className="mx-auto p-6 w-4/7">
      <button
        onClick={() => navigate(-1)}
        className="my-6 flex items-center text-grey-600 hover:text-orange-600 cursor-pointer transition focus:outline-none"
      >
        <FaArrowLeft className="mr-2" />
        <span className="sr-only sm:not-sr-only sm:inline">Natrag</span>
      </button>
      <h2 className="text-2xl font-bold mb-4">Objava novog recepta</h2>
      {error && <p className="text-red-500 mb-4">{error}</p>}

      <Formik
        initialValues={{
          name: "",
          ingredients: "",
          instructions: "",
          preparationTime: "",
          categoryId: "",
          image: null,
        }}
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
      >
        {({ setFieldValue, values }) => {
          /* dropzone hook MUST live inside render, to access setFieldValue */
          const { getRootProps, getInputProps } = makeDropzone(setFieldValue);

          return (
            <Form className="space-y-4">
              {/* Naziv */}
              <div>
                <label htmlFor="name" className="block mb-2">
                  Naziv recepta
                </label>
                <Field
                  type="text"
                  id="name"
                  name="name"
                  className="w-full p-2 border rounded-md"
                />
                <ErrorMessage
                  name="name"
                  component="p"
                  className="text-red-500 text-sm"
                />
              </div>

              {/* Sastojci */}
              <div>
                <label htmlFor="ingredients" className="block mb-2">
                  Sastojci {""}
                  <span className="text-xs text-gray-500">
                    (Sastojke odvojite zarezima)
                  </span>
                </label>
                <Field
                  as="textarea"
                  id="ingredients"
                  name="ingredients"
                  className="w-full p-2 border rounded-md"
                />
                <ErrorMessage
                  name="ingredients"
                  component="p"
                  className="text-red-500 text-sm"
                />
              </div>

              {/* Upute */}
              <div>
                <label htmlFor="instructions" className="block mb-2">
                  Upute {""}
                  <span className="text-xs text-gray-500">
                    (svaki korak pripreme odvojite u novi red)
                  </span>
                </label>
                <Field
                  as="textarea"
                  id="instructions"
                  name="instructions"
                  className="w-full p-2 border rounded-md"
                />
                <ErrorMessage
                  name="instructions"
                  component="p"
                  className="text-red-500 text-sm"
                />
              </div>

              {/* Vrijeme pripreme */}
              <div>
                <label htmlFor="preparationTime" className="block mb-2">
                  Vrijeme pripreme (u minutama)
                </label>
                <Field
                  type="number"
                  id="preparationTime"
                  name="preparationTime"
                  className="w-full p-2 border rounded-md"
                />
                <ErrorMessage
                  name="preparationTime"
                  component="p"
                  className="text-red-500 text-sm"
                />
              </div>

              {/* Kategorija */}
              <div>
                <label htmlFor="categoryId" className="block mb-2">
                  Kategorija jela
                </label>
                <Field
                  as="select"
                  id="categoryId"
                  name="categoryId"
                  className="w-full p-2 border rounded-md"
                >
                  <option value="">Odaberite kategoriju</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.name}
                    </option>
                  ))}
                </Field>
                <ErrorMessage
                  name="categoryId"
                  component="p"
                  className="text-red-500 text-sm"
                />
              </div>

              {/* Slika */}
              <div>
                <label className="block mb-2" htmlFor="image">
                  Slika
                </label>
                <div
                  {...getRootProps()}
                  className="border-2 border-dashed p-6 text-center rounded-md cursor-pointer"
                >
                  <input {...getInputProps()} id="image" />
                  {values.image ? (
                    <p className="text-green-500">{values.image.name}</p>
                  ) : (
                    <p className="text-gray-400">
                      Mišem odvucite datoteku slike ovdje ili kliknite za
                      biranje datoteke kroz eksplorer
                    </p>
                  )}
                </div>
                <ErrorMessage
                  name="image"
                  component="p"
                  className="text-red-500 text-sm"
                />
              </div>

              {/* Submit */}
              <div className="mt-4">
                <button
                  type="submit"
                  className={`px-6 py-2 text-white cursor-pointer rounded-md ${
                    loading
                      ? "bg-gray-400"
                      : "bg-orange-500 hover:bg-orange-600"
                  }`}
                  disabled={loading}
                >
                  {loading ? "Objavljujem recept..." : "Objavi recept"}
                </button>
              </div>
            </Form>
          );
        }}
      </Formik>
    </div>
  );
}
