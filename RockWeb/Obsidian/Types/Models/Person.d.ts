import Entity from './Entity';

export default interface Person extends Entity {
    FirstName: string;
    LastName: string;
    PhotoUrl: string;
    FullName: string;
}