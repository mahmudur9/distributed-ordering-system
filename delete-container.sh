sudo docker stop ordering-system-mssql product-service-api order-service-api payment-service-api user-service-api object-store-service-api
sudo docker rm ordering-system-mssql product-service-api order-service-api payment-service-api user-service-api object-store-service-api
sudo docker rmi product-service order-service payment-service user-service object-store-service
