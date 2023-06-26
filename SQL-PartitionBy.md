## SQL Partition By command ##   

* Suppose we have modelled a transaction concept as a table in sql called Transactions with columns as follows: 

| Id | TransactionDate | Amount | CustomerId | CustomerName | CardNumber (hypothetical) |
| -- | -------------- | ------ | ----------| ------------ | -------------------------|
| 1  | 2023-01-01     | 1000   | 1         | Jack        | 1278                     |
| 1  | 2023-01-02     | 200    | 1         | Jack        | 1273                     |
| 1  | 2023-01-03     | 300    | 2         | Sara        | 1275                     |
| 1  | 2023-01-03     | 500    | 2         | Sara        | 1275                     |
| 1  | 2023-01-03     | 2200   | 2         | Sara        | 1275                     |

---
* Our first requirement is to give a report of the total amount of each customers transactions.

```
SELECT CustomerId, 
       SUM(Amount) TotalAmout
FROM Transactions
GROUP BY CustomerId 
```   

| CustomerId | TotalAmount |
| ---------- | ----------- |
| 1          | 1200        |
| 2          | 3000        |

---  
* A new requirement comes in, we need the report to contain names of each customer, we do not want to use join or subquery as it will make things slow. We can't use group by anymore since we need columns other than the group by column. 
````
SELECT CustomerId, 
       CustomerName,
       SUM(Amount) OVER (PARTITION BY CustomerId) TotalAmout
FROM Transactions
````   

The result would be:
| CustomerId | CustomerName | TotalAmount |
| ---------- | ------------ | ----------- |
| 1          | Jack         | 1200        |
| 1          | Jack         | 1200        |
| 2          | Sara         | 3000        |
| 2          | Sara         | 3000        |
| 2          | Sara         | 3000        |


---

* OK, but we need only one row for each customer. And One more thing, we need a the card number of the latest transaction.
So we are going to use ROW_NUMBER method to number each record and CTE to make our query more flexible:

````
;WITH TX_CTE AS(
SELECT CustomerId, 
       CustomerName,
       CardNumber,
       SUM(Amount) OVER (PARTITION BY CustomerId) TotalAmout,
	   ROW_NUMBER() OVER (PARTITION BY CustomerId ORDER BY TransactionDate desc) RowNumber
FROM Transactions)
SELECT * FROM TX_CTE WHERE RowNumber=1
````   

The result would be:   
| CustomerId | CustomerName | TotalAmount | CardNumber |
| ---------- | ------------ | ----------- | ----------|
| 1          | Jack         | 1200        | 1273       |
| 2          | Sara         | 3000        | 1275       |
