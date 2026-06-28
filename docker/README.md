# Docker Images

This repo builds three deployable images:

- `registry.hakiware.com/rapid-newsletter-backend:latest`
- `registry.hakiware.com/rapid-newsletter-frontend:latest` / `registry.hakiware.com/rapid-newsletter-admin:latest`
- `registry.hakiware.com/rapid-newsletter-reader:latest`

Build all images:

```sh
docker buildx bake
```

Build and push all images:

```sh
docker login registry.hakiware.com
docker buildx bake --push
```

Set a version tag:

```sh
TAG=2026-06-28 docker buildx bake --push
```

Set frontend API URLs at build time:

```sh
VITE_API_URL=https://api.example.com \
PUBLIC_API_URL=https://api.example.com \
docker buildx bake --push
```

Build individual images:

```sh
docker build -f Dockerfile.backend -t registry.hakiware.com/rapid-newsletter-backend:latest .
docker build -f Dockerfile.frontend --target admin-runner -t registry.hakiware.com/rapid-newsletter-frontend:latest -t registry.hakiware.com/rapid-newsletter-admin:latest .
docker build -f Dockerfile.frontend --target reader-runner -t registry.hakiware.com/rapid-newsletter-reader:latest .
```
