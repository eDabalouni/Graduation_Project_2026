export interface ProjectDto {
  id: number;
  name: string;
  description: string | null;
  createdAt: string;
  ownerId: string;
  ownerName: string;
  membersCount: number;
  tasksCount: number;
}

export interface ProjectCreateDto {
  name: string;
  description?: string;
}

export interface ProjectUpdateDto {
  name: string;
  description?: string;
}