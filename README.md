# GlobalRepository

GlobalRepository is a "generic" repository that replace all the repositories in your project, in this way you will just need to use one and unique repository which mean to maintain one repository. With GlobalRepository you don't need to create a new repository for each Model and define methods ! just add and use it.

Please note :
- I called It GLOBAL repository not GENERIC because the objective is to use one repository for all the models, it's like you call it GLOBALLY in your project, in this case you will avoid tons of code (of course you are free to change it and implement it like GENERICE repository)

- The methods returns IQueryable which gives the most flexibility and allows for efficient querying as opposed to in-memory filtering, etc, and could reduce the need for making a ton of specific data fetching methods. On the other hand, now you have given your users a shotgun. They can do things which you may not have intended (overusing .include(), doing heavy heavy queries and doing in-memory filtering in their respective implementations, etc), which would basically side-step the layering and behavioral controls because you have given full access. So please before use take in consideration the team, their experience, the size of the app, the overall layering and architecture...

GlobalRepository work anywhere, just download the files, add them to your project and happy use :D 

please remember :
- Don't forget to change the name space...
- Don't forget to add the repository to your services in startup.cs
- If you have BaseEntity.cs class with another name just change the name in the donwloaded files.

PS: to work correctly, your Models must extends BaseEntity, I added a file so You can see how BaseEntity look like (for newbies)

If you need something please contact me at: mabroukifakhri@gmail.com
