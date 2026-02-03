import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: ".",
  timeout: 30_000,

  use: {
    baseURL: "http://localhost:3040",
    headless: true,
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
  },

  projects: [
    { name: "Chromium", use: { browserName: "chromium" } },
    { name: "Firefox", use: { browserName: "firefox" } },
    { name: "WebKit", use: { browserName: "webkit" } },
  ],
});
