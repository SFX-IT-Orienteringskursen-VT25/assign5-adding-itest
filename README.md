# assign5-adding-itest
The assignment was introduced in Class #10. In the link you can find the slides from the presentation as well as a recording of the class.
https://classroom.google.com/c/MjI5OTg3MzMwODFa/p/MjE3NjAxMzMwMjRa/details

Example of integration tests. Sets up SQL server using docker.
https://github.com/SFX-IT-Orienteringskursen-VT25/integration-test-example

## Instructions
Don't start this assignment until you have finshed assignment 4!

Clone this repository

Reuse your application from assignment 4.

Create a branch

Requirements:
- The Api should be integration tested
  - add “enough” tests for your endpoints
  - remember “negative” tests (mentioned in Class #6 https://classroom.google.com/c/MjI5OTg3MzMwODFa/p/MjE0Njg4NTg4MDJa/details)
  - The Api has to use a “real” database (database container, not a mock)
  - Integration testing in this context means invoking the api using its endpoints
- Tests should be able to be executed with one command (i.e. dotnet test/npm test/etc)
  - this means that the dependencies have to be setup from within the tests (database container cannot be initialized externally)

Commit

Push your branch

Create a pull request `<your branch> => <main>`

Done!
