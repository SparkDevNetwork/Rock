<template>
    
    <v-container fluid>
             <iframe  :src="src" frameborder="0"></iframe>
    </v-container>
</template>

<script>
export default {
    props:{
        src: String
    },
    methods:{
    
    }
}


</script>
<style >
 iframe{

    overflow:hidden;
    min-height:85vh;
    width:100%;
    /* margin-top:50px; */
    border:0px;
    }
</style>