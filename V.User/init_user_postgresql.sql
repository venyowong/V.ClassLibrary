-- Table: public.user

DROP TABLE IF EXISTS public."user";
CREATE SEQUENCE IF NOT EXISTS user_id_seq;

CREATE TABLE IF NOT EXISTS public."user"
(
    id bigint NOT NULL DEFAULT nextval('user_id_seq'::regclass),
    name character varying(200) COLLATE pg_catalog."default",
    avatar character varying(500) COLLATE pg_catalog."default",
    source character varying(50) COLLATE pg_catalog."default" NOT NULL,
    source_name character varying(50) COLLATE pg_catalog."default",
    platform_id character varying(100) COLLATE pg_catalog."default",
    mail character varying(50) COLLATE pg_catalog."default",
    location character varying(100) COLLATE pg_catalog."default",
    company character varying(100) COLLATE pg_catalog."default",
    bio character varying(1000) COLLATE pg_catalog."default",
    gender integer,
    password character varying(64) COLLATE pg_catalog."default",
    salt character varying(64) COLLATE pg_catalog."default",
    is_valid boolean NOT NULL DEFAULT true,
    create_time timestamp with time zone NOT NULL DEFAULT now(),
    update_time timestamp with time zone NOT NULL DEFAULT now(),
    mask_mobile character varying(11) COLLATE pg_catalog."default",
    md5_mobile character varying(32) COLLATE pg_catalog."default",
    encrypted_mobile character varying(32) COLLATE pg_catalog."default",
    CONSTRAINT user_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."user"
    OWNER to postgres;
-- Index: user_mail_idx

DROP INDEX IF EXISTS public.user_mail_idx;

CREATE INDEX IF NOT EXISTS user_mail_idx
    ON public."user" USING btree
    (mail COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: user_mobile_idx

DROP INDEX IF EXISTS public.user_mobile_idx;

CREATE INDEX IF NOT EXISTS user_mobile_idx
    ON public."user" USING btree
    (md5_mobile COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: user_platform_id_idx

DROP INDEX IF EXISTS public.user_platform_id_idx;

CREATE INDEX IF NOT EXISTS user_platform_id_idx
    ON public."user" USING btree
    (source COLLATE pg_catalog."default" ASC NULLS LAST, platform_id COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;