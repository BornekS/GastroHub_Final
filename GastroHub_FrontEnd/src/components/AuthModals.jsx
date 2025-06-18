import React from "react";
import { Dialog } from "./Dialog";
import { Input } from "./Input";
import { Button } from "./Button";
import { useToast } from "./ToastContext";
import { useAuth } from "./AuthContext";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";

// Basic sanitization function
const sanitize = (str) =>
  str.trim().replace(/</g, "&lt;").replace(/>/g, "&gt;");

export function AuthModals({ type, isOpen, onClose }) {
  const { showToast } = useToast();
  const { login } = useAuth();

  const isRegister = type === "register";

  const initialValues = {
    email: "",
    password: "",
    confirmPassword: "",
    displayName: "",
  };

  const validationSchema = Yup.object({
    email: Yup.string().email("Nevažeći email").required("Email je obavezan"),
    password: Yup.string()
      .min(6, "Minimalno 6 znakova")
      .required("Lozinka je obavezna"),
    ...(isRegister && {
      confirmPassword: Yup.string()
        .oneOf([Yup.ref("password"), null], "Lozinke se moraju podudarati")
        .required("Potvrdite lozinku"),
      displayName: Yup.string().min(2, "Prekratko").required("Ime je obavezno"),
    }),
  });

  const handleSubmit = async (values, { setSubmitting }) => {
    const endpoint = isRegister
      ? "http://localhost:5292/api/Auth/register"
      : "http://localhost:5292/api/Auth/login";

    const payload = isRegister
      ? {
          email: sanitize(values.email),
          password: sanitize(values.password),
          confirmPassword: sanitize(values.confirmPassword),
          displayName: sanitize(values.displayName),
        }
      : {
          email: sanitize(values.email),
          password: sanitize(values.password),
        };

    try {
      const res = await fetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Greška na serveru");
      }

      const data = await res.json();

      // Store user in context (and localStorage)
      login(data);

      showToast(
        isRegister ? "Registracija uspješna!" : "Prijava uspješna!",
        "success"
      );
      console.log(`${type} success:`, data);
      onClose();
    } catch (err) {
      console.error(`${type} failed:`, err);
      showToast(
        isRegister ? "Registracija nije uspjela." : "Prijava nije uspjela.",
        "error"
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog isOpen={isOpen} onClose={onClose}>
      <h2 className="text-xl font-semibold mb-4 capitalize">
        {isRegister ? "Registracija" : "Prijava"}
      </h2>

      <Formik
        initialValues={initialValues}
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
          <Form className="space-y-4">
            <div>
              <label className="block mb-1 text-sm font-medium">Email</label>
              <Field name="email" as={Input} placeholder="Unesite email" />
              <ErrorMessage
                name="email"
                component="div"
                className="text-red-500 text-sm mt-1"
              />
            </div>

            <div>
              <label className="block mb-1 text-sm font-medium">Lozinka</label>
              <Field
                name="password"
                type="password"
                as={Input}
                placeholder="Unesite lozinku"
              />
              <ErrorMessage
                name="password"
                component="div"
                className="text-red-500 text-sm mt-1"
              />
            </div>

            {isRegister && (
              <>
                <div>
                  <label className="block mb-1 text-sm font-medium">
                    Potvrdite lozinku
                  </label>
                  <Field
                    name="confirmPassword"
                    type="password"
                    as={Input}
                    placeholder="Ponovno unesite lozinku"
                  />
                  <ErrorMessage
                    name="confirmPassword"
                    component="div"
                    className="text-red-500 text-sm mt-1"
                  />
                </div>

                <div>
                  <label className="block mb-1 text-sm font-medium">
                    Korisničko ime
                  </label>
                  <Field
                    name="displayName"
                    as={Input}
                    placeholder="Unesite korisničko ime"
                  />
                  <ErrorMessage
                    name="displayName"
                    component="div"
                    className="text-red-500 text-sm mt-1"
                  />
                </div>
              </>
            )}

            <Button type="submit" disabled={isSubmitting}>
              {isRegister ? "Registriraj se" : "Prijavi se"}
            </Button>
          </Form>
        )}
      </Formik>
    </Dialog>
  );
}
