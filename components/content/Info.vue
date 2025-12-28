<script setup lang="ts"></script>

<template>
    <details open>
        <summary print-exact><slot name="head"></slot></summary>
        <p>
            <slot name="content"></slot>
        </p>
    </details>
</template>

<style scoped>
    details {
        margin-top: 1em;
        margin-bottom: 1em;
        font-size: calc(1px * var(--size-minus-2));
        padding: calc(1em * var(--padding-container-factor));
        background-color: var(--color-khaki-w1);
        border-width: calc(1em * var(--border-width-factor));
        border-style: var(--border-style);
        border-color: var(--color-brown);
    }

    details summary > * {
        display: inline;
        font-size: calc(1px * var(--size-minus-1));
        padding: calc(1em * var(--padding-container-factor));
    }

    details summary::marker {
        content: "";
    }

    details summary {
        width: fit-content;
        background-color: var(--color-slate);
        color: var(--color-khaki-w1);
        font-weight: bold;
        margin-left: calc(-1em * var(--padding-container-factor));

        @media print {
            color: white;
        }
    }
</style>
