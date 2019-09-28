# IntegrityML
System supporting the development of machine learning models that analyse Monitoring data

This system is a moicroservice based system. To run it RabbitMQ as the message queue and MongoDB as the database is needed. 

Setting up the system:
Start MongoDB and add the collections: asd and ads.
Start RabbitMQ. LocalHost is used to connect.
Make sure the config file is there and the settings are right.
Start the Calculation, the InterpretationML and the CalcSim app.
When starting the CalcSim app, make sure to pass the path of the config file.
