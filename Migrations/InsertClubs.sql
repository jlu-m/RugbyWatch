insert into clubs (Name) values ('Aeron�uticos');
insert into clubs (Name) values ('Ammonites RFUC Segovia');
insert into clubs (Name) values ('Arquitectura');
insert into clubs (Name) values ('BigMat Tabanera Lobos Segovia');
insert into clubs (Name) values ('XV Hortaleza - CAU');
insert into clubs (Name) values ('CCVK Vallecas Rugby Union');
insert into clubs (Name) values ('Club Atl�tico Rivas');
insert into clubs (Name) values ('Club de Rugby Majadahonda');
insert into clubs (Name) values ('Club de Rugby Veterinaria');
insert into clubs (Name) values ('Club Rugby Alcal�');
insert into clubs (Name) values ('Complutense Cisneros');
insert into clubs (Name) values ('CR Liceo Franc�s');
insert into clubs (Name) values ('CRC Pozuelo');
insert into clubs (Name) values ('FILO Rugby Club');
insert into clubs (Name) values ('Getafe Club de Rugby');
insert into clubs (Name) values ('Hermo Soto del Real');
insert into clubs (Name) values ('Industriales Las Rozas');
insert into clubs (Name) values ('Jabatos M�stoles R.C.');
insert into clubs (Name) values ('Liceo Filo');
insert into clubs (Name) values ('MAD Boadilla');
insert into clubs (Name) values ('Madrid Barbarians RFC');
insert into clubs (Name) values ('Madrid Titanes C.R.');
insert into clubs (Name) values ('Ol�mpico de Pozuelo Tasman');
insert into clubs (Name) values ('Osos del Pardo ');
insert into clubs (Name) values ('PDM Rugby Toledo');
insert into clubs (Name) values ('Quijote Rugby Club');
insert into clubs (Name) values ('Rugby Alcorc�n');
insert into clubs (Name) values ('Rugby Guadalajara XV Jabal�');
insert into clubs (Name) values ('RUN Rugby Uni�n Norte');
insert into clubs (Name) values ('San Isidro Fuencarral');
insert into clubs (Name) values ('Sanse Scrum');
insert into clubs (Name) values ('Silicius Alcobendas ');
insert into clubs (Name) values ('Tasman Ol�mpico Boadilla');
insert into clubs (Name) values ('Torrelodones RC');
insert into clubs (Name) values ('Villalba Dwarfs');


UPDATE t
SET t.ClubId = c.Id
FROM Teams t
INNER JOIN Clubs c ON LEFT(t.Name, 10) = LEFT(c.Name, 10);