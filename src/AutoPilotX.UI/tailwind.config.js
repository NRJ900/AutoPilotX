/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "#1e1e1e",
        surface: "#2d2d30",
        primary: "#007acc",
        secondary: "#a0a0a0",
        danger: "#f44b56",
        success: "#57bd6a"
      }
    },
  },
  plugins: [],
}
