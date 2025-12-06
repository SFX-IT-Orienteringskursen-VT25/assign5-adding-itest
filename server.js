import app from "./app.js";
import { connectWithRetry } from "./db-connect.js";
import { initDb } from "./db-init.js";

async function startServer() {
  try {
    await initDb();

    await connectWithRetry();

    const PORT = process.env.PORT || 4000;
    app.listen(PORT, () =>
      console.log(`🚀 API running at http://localhost:${PORT}`)
    );
  } catch (err) {
    console.error("❌ FATAL: Could not start server:", err);
    process.exit(1);
  }
}

if (process.env.NODE_ENV !== "test") {
  startServer();
}

export { startServer };
