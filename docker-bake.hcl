variable "REGISTRY" {
  default = "registry.hakiware.com"
}

variable "TAG" {
  default = "latest"
}

variable "VITE_API_URL" {
  default = "http://localhost:5120"
}

variable "PUBLIC_API_URL" {
  default = "http://localhost:5120"
}

group "default" {
  targets = ["backend", "frontend-admin", "frontend-reader"]
}

target "backend" {
  context    = "."
  dockerfile = "Dockerfile.backend"
  tags       = ["${REGISTRY}/rapid-newsletter-backend:${TAG}"]
}

target "frontend-admin" {
  context    = "."
  dockerfile = "Dockerfile.frontend"
  target     = "admin-runner"
  args = {
    VITE_API_URL = "${VITE_API_URL}"
  }
  tags = [
    "${REGISTRY}/rapid-newsletter-frontend:${TAG}",
    "${REGISTRY}/rapid-newsletter-admin:${TAG}"
  ]
}

target "frontend-reader" {
  context    = "."
  dockerfile = "Dockerfile.frontend"
  target     = "reader-runner"
  args = {
    PUBLIC_API_URL = "${PUBLIC_API_URL}"
  }
  tags = ["${REGISTRY}/rapid-newsletter-reader:${TAG}"]
}
