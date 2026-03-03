FROM node:20-alpine3.23

WORKDIR /app

ENV PNPM_VERSION=10.30.3
ENV PNPM_HOME="/root/.local/share/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN wget -qO- https://get.pnpm.io/install.sh | ENV="$HOME/.shrc" SHELL="$(which sh)" sh -

COPY package.json pnpm-lock.yaml ./

RUN pnpm install

EXPOSE 5173

CMD ["pnpm", "run", "dev", "--host"]
