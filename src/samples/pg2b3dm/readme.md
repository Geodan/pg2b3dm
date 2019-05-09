# pg2b3dm

Tool for converting from PostGIS to b3dm tiles.

## Docker

Building image:

```
$ docker build -t geodan/pg2b3dm .
```

Running image:

Sample on Windows: 

```
$ docker run -v C:/Users/bertt/tiles:/app/tiles -it geodan/pg2b3dm
```

Sample on Linux:

```
$ docker run -v $(pwd)/output:/app/output -it geodan/pg2b3dm -H my_host -u my_user -p my_password -D my_database -t my_table -c my_geometry_column
```

## Command line options

```
 -H, --host        Required. Database host

  -D, --database    Required. Database name

  -c, --column      Required. Geometry column

  -t, --table       Required. Database table

  -u, --user        Required. Database user

  -p, --password    Required. Database password

  --help            Display this help screen.

  --version         Display version information.
  ```
