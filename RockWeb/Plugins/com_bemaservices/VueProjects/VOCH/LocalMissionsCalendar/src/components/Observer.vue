<template>
  <div class="observer"/>
</template>
  
<script>
export default {
// Create Data 
data: () => ({
  observer:null
}),
// When Mounted
mounted() {
   this.observer = new IntersectionObserver(([entry]) => {
      if (entry && entry.isIntersecting) {
         this.$emit("intersect");
      }
    });

    this.observer.observe(this.$el);
},

destroyed() {
    this.observer.disconnect();
  }

}
</script>

<style>

</style>