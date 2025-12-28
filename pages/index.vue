<script setup lang="ts">
    definePageMeta({
        layout: "cv"
    });

    const {data: page} = await useAsyncData(() => {
        return queryCollection("content").path("/cv").first();
    });

    if (!page.value) {
        throw createError({statusCode: 404, statusMessage: "Page not found", fatal: true});
    }
</script>

<template>
    <ContentRenderer v-if="page" :value="page" />
</template>
