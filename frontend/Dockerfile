FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
ARG VITE_GOOGLE_MAPS_API_KEY
RUN npm run build

FROM alpine:latest
WORKDIR /app
COPY --from=build /app/dist .
