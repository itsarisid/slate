const specUrl = "/openapi/v1.json";
const credentials = {
  bearer: localStorage.getItem("alphabet.docs.bearer") || "",
  apiKey: localStorage.getItem("alphabet.docs.apiKey") || "",
  oauthClientId: localStorage.getItem("alphabet.docs.oauthClientId") || ""
};

const examples = {
  curl: `curl -X GET "https://localhost:58441/health" \\
  -H "Authorization: Bearer $ALPHABET_TOKEN"`,
  csharp: `using var client = new HttpClient { BaseAddress = new Uri("https://localhost:58441") };
client.DefaultRequestHeaders.Authorization = new("Bearer", token);
var response = await client.GetFromJsonAsync<object>("/api/v1/assets");`,
  typescript: `const response = await fetch("https://localhost:58441/api/v1/assets", {
  headers: { Authorization: \`Bearer \${token}\` }
});
const data = await response.json();`,
  python: `import requests

response = requests.get(
    "https://localhost:58441/api/v1/assets",
    headers={"Authorization": f"Bearer {token}"}
)
print(response.json())`,
  java: `HttpRequest request = HttpRequest.newBuilder()
    .uri(URI.create("https://localhost:58441/api/v1/assets"))
    .header("Authorization", "Bearer " + token)
    .GET()
    .build();`,
  go: `req, _ := http.NewRequest("GET", "https://localhost:58441/api/v1/assets", nil)
req.Header.Set("Authorization", "Bearer "+token)
res, err := http.DefaultClient.Do(req)`,
  php: `$client = new GuzzleHttp\\Client(["base_uri" => "https://localhost:58441"]);
$response = $client->get("/api/v1/assets", [
  "headers" => ["Authorization" => "Bearer " . $token]
]);`,
  ruby: `uri = URI("https://localhost:58441/api/v1/assets")
request = Net::HTTP::Get.new(uri)
request["Authorization"] = "Bearer #{token}"`,
  kotlin: `val request = Request.Builder()
  .url("https://localhost:58441/api/v1/assets")
  .header("Authorization", "Bearer $token")
  .build()`,
  swift: `var request = URLRequest(url: URL(string: "https://localhost:58441/api/v1/assets")!)
request.setValue("Bearer \\(token)", forHTTPHeaderField: "Authorization")
let (data, _) = try await URLSession.shared.data(for: request)`
};

const viewerMount = document.getElementById("viewerMount");
const languageSelect = document.getElementById("languageSelect");
const codeExample = document.getElementById("codeExample");

function authHeaders() {
  const headers = {};
  if (credentials.bearer) headers.Authorization = `Bearer ${credentials.bearer}`;
  if (credentials.apiKey) headers["x-api-key"] = credentials.apiKey;
  return headers;
}

function renderSwagger() {
  viewerMount.innerHTML = '<div id="swagger-ui"></div>';
  window.SwaggerUIBundle({
    url: specUrl,
    dom_id: "#swagger-ui",
    deepLinking: true,
    persistAuthorization: true,
    requestInterceptor: request => {
      Object.assign(request.headers, authHeaders());
      return request;
    }
  });
}

function renderRedoc() {
  viewerMount.innerHTML = '<div id="redoc"></div>';
  window.Redoc.init(specUrl, {
    hideDownloadButton: false,
    nativeScrollbars: true,
    theme: {
      colors: { primary: { main: "#0f766e" } },
      typography: { fontFamily: "Inter, system-ui, sans-serif" }
    }
  }, document.getElementById("redoc"));
}

function renderScalar() {
  viewerMount.innerHTML = `<script type="application/json" data-configuration>
{
  "url": "${specUrl}",
  "theme": "default",
  "layout": "modern",
  "hideDownloadButton": false
}
</script><scalar-api-reference></scalar-api-reference>`;
}

function renderRapidoc() {
  viewerMount.innerHTML = `<rapi-doc
    spec-url="${specUrl}"
    theme="auto"
    render-style="read"
    layout="row"
    show-header="false"
    allow-authentication="true"
    allow-try="true"
    persist-auth="true"
  ></rapi-doc>`;
}

function renderViewer(name) {
  ({ swagger: renderSwagger, redoc: renderRedoc, scalar: renderScalar, rapidoc: renderRapidoc }[name] || renderSwagger)();
}

document.querySelectorAll("[data-viewer]").forEach(button => {
  button.addEventListener("click", () => {
    document.querySelectorAll("[data-viewer]").forEach(item => item.classList.remove("active"));
    button.classList.add("active");
    renderViewer(button.dataset.viewer);
  });
});

document.getElementById("authForm").addEventListener("submit", event => {
  event.preventDefault();
  credentials.bearer = document.getElementById("bearerToken").value.trim();
  credentials.apiKey = document.getElementById("apiKey").value.trim();
  credentials.oauthClientId = document.getElementById("oauthClientId").value.trim();
  localStorage.setItem("alphabet.docs.bearer", credentials.bearer);
  localStorage.setItem("alphabet.docs.apiKey", credentials.apiKey);
  localStorage.setItem("alphabet.docs.oauthClientId", credentials.oauthClientId);
  renderViewer(document.querySelector("[data-viewer].active").dataset.viewer);
});

document.getElementById("bearerToken").value = credentials.bearer;
document.getElementById("apiKey").value = credentials.apiKey;
document.getElementById("oauthClientId").value = credentials.oauthClientId;

document.getElementById("consoleForm").addEventListener("submit", async event => {
  event.preventDefault();
  const method = document.getElementById("method").value;
  const path = document.getElementById("path").value;
  const bodyText = document.getElementById("requestBody").value.trim();
  const output = document.getElementById("consoleOutput");
  output.textContent = "Sending...";

  try {
    const response = await fetch(path, {
      method,
      headers: {
        "content-type": "application/json",
        ...authHeaders()
      },
      body: ["GET", "DELETE"].includes(method) || !bodyText ? undefined : bodyText
    });
    const text = await response.text();
    output.textContent = `${response.status} ${response.statusText}\n\n${prettyJson(text)}`;
  } catch (error) {
    output.textContent = error instanceof Error ? error.message : String(error);
  }
});

function prettyJson(text) {
  try {
    return JSON.stringify(JSON.parse(text), null, 2);
  } catch {
    return text || "(empty response)";
  }
}

Object.keys(examples).forEach(language => {
  const option = document.createElement("option");
  option.value = language;
  option.textContent = language;
  languageSelect.appendChild(option);
});

languageSelect.addEventListener("change", () => {
  codeExample.textContent = examples[languageSelect.value];
});
languageSelect.value = "curl";
codeExample.textContent = examples.curl;

document.getElementById("searchInput").addEventListener("input", event => {
  const query = event.target.value.trim().toLowerCase();
  document.querySelectorAll(".searchable").forEach(section => {
    const haystack = `${section.textContent} ${section.dataset.search || ""}`.toLowerCase();
    section.classList.toggle("hidden", query.length > 0 && !haystack.includes(query));
  });
});

document.querySelectorAll("[data-sdk]").forEach(button => {
  button.addEventListener("click", () => {
    const sdk = button.dataset.sdk;
    const command = `openapi-generator-cli generate -i https://localhost:58441/openapi/v1.json -g ${sdk} -o ./sdk/alphabet-${sdk}`;
    navigator.clipboard?.writeText(command);
    button.textContent = "Command copied";
    setTimeout(() => {
      button.textContent = `${sdk.replace("-", " ")} SDK`;
    }, 1600);
  });
});

fetch(specUrl)
  .then(response => response.json())
  .then(spec => {
    const paths = spec.paths || {};
    const count = Object.values(paths).reduce((total, pathItem) => {
      return total + Object.keys(pathItem).filter(key => ["get", "post", "put", "patch", "delete"].includes(key)).length;
    }, 0);
    document.getElementById("endpointCount").textContent = count;
  })
  .catch(() => {
    document.getElementById("endpointCount").textContent = "Live";
  });

renderSwagger();
