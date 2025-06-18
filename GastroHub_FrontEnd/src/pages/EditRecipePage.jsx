import React, { useEffect, useState, useCallback } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useDropzone } from "react-dropzone";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import { FaArrowLeft } from "react-icons/fa";
import { useToast } from "../components/ToastContext";

/* ————— Validation ————— */
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
  // slika nije obavezna kod editiranja
  image: Yup.mixed().nullable(),
});

export default function EditRecipePage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const { showToast } = useToast();

  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [initialValues, setInitialValues] = useState(null);

  const authUser = JSON.parse(localStorage.getItem("authUser"));
  const token = authUser?.token;

  /* ————— Fetch categories & recipe data ————— */
  useEffect(() => {
    // categories
    fetch("http://localhost:5292/api/Categories")
      .then((r) => (r.ok ? r.json() : Promise.reject()))
      .then(setCategories)
      .catch(() => showToast("Greška pri učitavanju kategorija", "error"));

    // recipe details
    fetch(`http://localhost:5292/api/Recipes/${id}`)
      .then((r) => (r.ok ? r.json() : Promise.reject()))
      .then((data) =>
        setInitialValues({
          name: data.name,
          ingredients: data.ingredients,
          instructions: data.instructions,
          preparationTime: data.preparationTimeMinutes,
          categoryId: data.categoryId,
          image: null, // File object to upload (optional)
          existingImageUrl: data.imageUrls?.[0] || "",
        })
      )
      .catch(() => {
        showToast("Neuspjelo učitavanje recepta", "error");
        navigate(-1);
      });
  }, [id, navigate, showToast]);

  /* ————— Dropzone ————— */
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

  /* ————— Submit ————— */
  const handleSubmit = async (values) => {
    setLoading(true);
    setError("");

    try {
      let imageUrl = values.existingImageUrl;

      // ako je odabran novi fajl ➜ upload
      if (values.image instanceof File) {
        const formData = new FormData();
        formData.append("file", values.image);
        const imgRes = await fetch("http://localhost:5292/api/Media/upload", {
          method: "POST",
          body: formData,
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!imgRes.ok) throw new Error("Upload slike nije uspio");
        const { url } = await imgRes.json();
        imageUrl = url;
      }

      const payload = {
        id: parseInt(id, 10),
        name: values.name,
        ingredients: values.ingredients,
        instructions: values.instructions,
        preparationTimeMinutes: parseInt(values.preparationTime, 10),
        categoryId: parseInt(values.categoryId, 10),
        imageUrl,
      };

      const res = await fetch(`http://localhost:5292/api/Recipes/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });
      if (!res.ok) throw new Error("Greška pri spremanju izmjena");

      showToast("Recept ažuriran", "success");
      navigate(-1);
    } catch (err) {
      const msg = err.message || "Greška pri spremanju recepta";
      setError(msg);
      showToast(msg, "error");
    } finally {
      setLoading(false);
    }
  };

  if (!initialValues) return <p className="p-6">Učitavanje...</p>;

  /* ————— UI ————— */
  return (
    <div className="mx-auto p-6 w-4/7">
      <button
        onClick={() => navigate(-1)}
        className="my-6 flex items-center text-grey-600 hover:text-orange-600 cursor-pointer transition focus:outline-none"
      >
        <FaArrowLeft className="mr-2" />
        <span className="sr-only sm:not-sr-only sm:inline">Natrag</span>
      </button>

      <h2 className="text-2xl font-bold mb-4">Uredi recept</h2>
      {error && <p className="text-red-500 mb-4">{error}</p>}

      <Formik
        initialValues={initialValues}
        enableReinitialize
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
      >
        {({ setFieldValue, values }) => {
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
                  Sastojci{" "}
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
                  Upute{" "}
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
                    <>
                      {values.existingImageUrl ? (
                        <img
                          src={`http://localhost:5292${values.existingImageUrl}`}
                          alt="trenutna slika"
                          className="mx-auto h-40 object-cover mb-2 rounded"
                        />
                      ) : null}
                      <p className="text-gray-400">
                        Povucite novu sliku ili kliknite za odabir (ostavite
                        prazno ako ne mijenjate)
                      </p>
                    </>
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
                  {loading ? "Spremam izmjene…" : "Spremi izmjene"}
                </button>
              </div>
            </Form>
          );
        }}
      </Formik>
    </div>
  );
}
