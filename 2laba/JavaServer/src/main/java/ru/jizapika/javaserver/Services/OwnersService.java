package ru.jizapika.javaserver.Services;

import ru.jizapika.javaserver.Objects.Owner;

import java.util.List;

public interface OwnersService {
    List<Owner> allOwners();
    public void create(Owner owner);
    public Owner read(int id);
    public void update(Owner owner, int id);
    public void delete(int id);

    public void addKitten(int id, int kittenId);
}