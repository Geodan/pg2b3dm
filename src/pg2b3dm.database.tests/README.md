# Database tests

To run these tests, run the Docker database with test table first.

Build:

```
$ docker build -t delaware .
```

Run:

```
$ docker run -e POSTGRES_PASSWORD=postgres -p 5432:5432 delaware
```