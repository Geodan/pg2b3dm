-- 3DCityDB v5 appearance graph, used by the --theme filter.
-- Additive on top of 5_create_3dcitydb_texture_tables.sql: a fourth geometry carrying two
-- surface_data (one per theme) so a theme selection can be told apart from "lowest
-- surface_data_id wins". Kept outside the bounding boxes the other texture tests query.

CREATE TABLE IF NOT EXISTS citydb.appearance
(
    id BIGINT PRIMARY KEY,
    objectid TEXT,
    theme TEXT
);

CREATE TABLE IF NOT EXISTS citydb.appear_to_surface_data
(
    id BIGSERIAL PRIMARY KEY,
    appearance_id BIGINT NOT NULL,
    surface_data_id BIGINT
);

INSERT INTO citydb.geometry_data (id, geometry, geometry_properties)
VALUES
    (
        4,
        'SRID=4326;POLYGON Z ((30 30 0, 31 30 0, 30 31 0, 30 30 0))'::geometry,
        '{"type": 6, "children": [{"type": 3, "objectId": "surface_4", "geometryIndex": 0}]}'::jsonb
    )
ON CONFLICT (id) DO NOTHING;

-- Two 1x1 PNGs with distinct pixels (red / blue), so a test can tell which theme was baked.
INSERT INTO citydb.tex_image (id, image_uri, mime_type, image_data)
VALUES
    (
        3,
        'red.png',
        'image/png',
        decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGP4z8AAAAMBAQDJ/pLvAAAAAElFTkSuQmCC', 'base64')
    ),
    (
        4,
        'blue.png',
        'image/png',
        decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGNgYPgPAAEDAQAIicLsAAAAAElFTkSuQmCC', 'base64')
    )
ON CONFLICT (id) DO NOTHING;

INSERT INTO citydb.surface_data (id, tex_image_id)
VALUES
    (4, 3),
    (5, 4)
ON CONFLICT (id) DO NOTHING;

-- Same surface of the same geometry, textured twice - the themes are what tell them apart.
INSERT INTO citydb.surface_data_mapping (geometry_data_id, surface_data_id, texture_mapping)
VALUES
    (
        4,
        4,
        '{"surface_4":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}'::jsonb
    ),
    (
        4,
        5,
        '{"surface_4":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}'::jsonb
    )
ON CONFLICT DO NOTHING;

-- 'summer' is carried by two appearances that both reference surface_data 4: the semi-join must
-- still yield one texture row, not one per appearance.
INSERT INTO citydb.appearance (id, objectid, theme)
VALUES
    (1, 'appearance_summer_a', 'summer'),
    (2, 'appearance_winter', 'winter'),
    (3, 'appearance_summer_b', 'summer')
ON CONFLICT (id) DO NOTHING;

INSERT INTO citydb.appear_to_surface_data (appearance_id, surface_data_id)
VALUES
    (1, 4),
    (3, 4),
    (2, 5)
ON CONFLICT DO NOTHING;
