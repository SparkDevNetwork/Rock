<template>
<v-container class="pt-12">

            <v-alert
                border="top"
                colored-border
                :color="fulfillmentType =='donation' ? 'success' : 'info'"
                elevation="2"
                :icon="fulfillmentType == 'donation' ? 'fa-money-bill-alt' : 'fa-shopping-cart-alt'"
                
                >
                <p>{{responseMessage}}</p>
                <div v-if="transactionInfo">
                    
                    <p>Transaction Details:</p>
                    <ul>
                        <li><strong>Total Amount: </strong>{{transactionInfo.TotalAmount}}</li>
                        <li><strong>Payment Type: </strong>{{transactionInfo.PaymentType}}</li>
                        <li><strong>Transaction Code: </strong>{{transactionInfo.TransactionCode}}</li>
                        <li><strong>Transaction Code: </strong>{{transactionInfo.TransactionDateTime}}</li>
                    </ul>
                </div>

                    <v-btn :class="fulfillmentType =='donation' ? 'success' : 'info'" @click="closeModal">
                    Close Window
                </v-btn>
            </v-alert>
        
                
        
</v-container>
</template>
<script>

export default {
    name: 'transactionComplete',
    props:{
        transactionInfo: Object,
        tagResponse: Object,
        fulfillmentType: String,
        responseMessage: String
    },
    methods:{
        closeModal(){
            this.$emit('closeModal')
        }
    }
}
</script>
<style scoped>

</style>