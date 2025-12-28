// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2024-11-01",
  modules: ["@nuxt/eslint", "@vueuse/nuxt", "@nuxt/content"],
  eslint: {
    config: {
      // https://eslint.style/packages/default
      stylistic: {
        semi: true
      }
    }
  },
  devtools: {
    enabled: true,
    timeline: {
      enabled: true
    }
  },
  typescript: {
    typeCheck: true
  },
  css: ["~/assets/base/style.css"],
  content: {
    build: {
      markdown: {
        remarkPlugins: {
          "remark-math": {
            singleDollarTextMath: true
          }
        },
        rehypePlugins: {
          "rehype-mathjax": {}
        }
      }
    }
  },
  ssr: true
});
