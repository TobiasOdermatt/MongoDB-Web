const PROXY_CONFIG = [
  {
    context: [
      "/api"
    ],
    target: "https://localhost:5010",
    secure: false,
    ws: true
  }
]

module.exports = PROXY_CONFIG;
