<template>
    <q-layout view="lHh Lpr lFf">
        <q-header elevated class="row bg-teal">
            <div class="col-md-8 offset-md-2 col-xs-12">
                <q-toolbar>
                    <q-toolbar-title> COVID CURE ID </q-toolbar-title>
                </q-toolbar>
            </div>
        </q-header>

        <q-page-container class="row">
            <div class="col-md-8 offset-md-2 col-xs-12">
                <router-view />
            </div>
        </q-page-container>
    </q-layout>
</template>

<script lang="ts">
import { defineComponent } from '@vue/composition-api';

export default defineComponent({
    name: 'MainLayout',
    components: {},
});
</script>

<style type="text/css">
</style>
