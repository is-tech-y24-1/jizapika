package ru.jizapika.javaserver.Dao;

import ru.jizapika.javaserver.Objects.Owner;

import java.util.List;

public interface OwnersDao {
    List<Owner> owners();
    public void create(Owner owner);

    public Owner read(int id);

    public void update(Owner owner, int id);

    public void delete(int id);

    public void addKitten(int id, int kittenId);
}
