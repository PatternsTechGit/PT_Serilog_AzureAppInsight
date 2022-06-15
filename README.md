# Serilog and Azure Application Insight

## What is Serilog and Azure Application Insight ?

First thing first, what is logging and why it is needed? Logging is the information that tells us what is happening in application and it is fundamental to troubleshoot any application problems.

Logging frameworks make it easy to send logs to different places via simple configurations, **Serilog** is one of the logging framework.

Serilog uses **sinks** which are the places where we store our log messages like text file, database, log management solutions(e.g. datadog), or some cloud service like **azure application insight** or potentially dozens of [other places](https://github.com/serilog/serilog/wiki/Provided-Sinks), all without changing your code.

In azure application insight we can also send *custom event* data, a custom event is a data point/metric in azure application insight that can be useful to identify a particular situation like how often users choose a particular feature, how often they achieve particular goals, or maybe make particular types of mistake.


---------------

## About this exercise

In this lab we will be working on two code Bases, **Backend Code base** 

### **Backend Code Base:**

Previously we developed a base structure of an api solution in Asp.net core that have just two api functions GetLast12MonthBalances & GetLast12MonthBalances/{userId} which returns data of the last 12 months total balances.

![](/BBBank_UI/src/assets/images/12m.jpg)


There are 4 Projects in the solution. 

*	**Entities** : This project contains DB models like *User* where each User has one *Account* and each Account can have one or many *Transaction*. There is also a Response Model of *LineGraphData* that will be returned as API Response. 

*	**Infrastructure**: This project contains *BBBankContext* that serves as fake DBContext that populates one User with its corresponding Account that has some Transactions dated of last twelve months with hardcoded data. 

* **Services**: This project contains *TransactionService* with the logic of converting Transactions into LineGraphData after fetching them from BBBankContext.

* **BBBankAPI**: This project contains *TransactionController* with two GET methods *GetLast12MonthBalances* & *GetLast12MonthBalances/{userId}* to call the *TransactionService*.

![](/BBBank_UI/src/assets/images/4.png)

For more details about this base project See: [Service Oriented Architecture Lab](https://github.com/PatternsTechGit/PT_ServiceOrientedArchitecture)


## In this exercise

* Install nuget packages to install Serilog
* Configure Serilog
* Send log data to azure application insight 
* Send custom event data to application insight
