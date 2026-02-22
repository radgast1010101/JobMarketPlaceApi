# JobMarketPlaceApi
---

1. git clone <repo>
2. start visual studio ide, open project solution
   
![step2-open-proj-Screenshot 2026-02-22 150624](https://github.com/user-attachments/assets/21c3704c-5a40-42b9-a1b3-f775ec2c4f5b)

   
![step3-select-sln-Screenshot 2026-02-22 150624](https://github.com/user-attachments/assets/e72b2373-58e7-4872-a237-55d9978055d2)

4. run "https", or build and run


![step4-run-https-Screenshot 2026-02-22 150624](https://github.com/user-attachments/assets/340e77e4-ccd3-4747-a22c-c4757fef1849)

6. other setup: 
4.a. SQLite database: At solution explorer, click "Connected Services"
Click Add "+" , Search for SQLite (local), Connect to db, Add Migration and Update Database

![sqlite-step-1-Screenshot 2026-02-22 151300](https://github.com/user-attachments/assets/d9377163-2f63-43f2-8ba6-978a87504deb)


![sqlite-step-2-Screenshot 2026-02-22 151300](https://github.com/user-attachments/assets/3e8469db-38a1-4ac9-8f5a-743b6aa5c0b2)
next, next(maybe edit dbcon string name), finished
then connect to db, add migration, update db, etc

4.b. Install dependencies: 
Go to "Project" menu-->Click, "Manage Nuget Packages"-->Install, "SQLite/SQL Server Compact Toolbox" (to manage sqlite db), "FluentAssertions", "Moq", "xunit", etc

4.c. Run Tests:
Go to "Test" menu-->Click, "Run All Test" or "Test Explorer"

5. Test in Swagger UI, have some fun, thanks

email me for any questions or bugs (or just add issue): <ub17@protonmail.com>
