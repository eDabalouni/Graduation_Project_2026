export interface TaskDto {
  id: number;
  title: string;
  description: string | null;
  status: 'Todo' | 'InProgress' | 'Done';
  priority: 'Low' | 'Medium' | 'High';
  dueDate: string | null;
  createdAt: string;
  projectId: number;
  assignedUserId: string | null;
  assignedUserName: string | null;
}

export interface TaskCreateDto {
  title: string;
  description?: string;
  priority: string;
  dueDate?: string | null;
  assignedUserId?: string | null;
}

export interface TaskStatusUpdateDto {
  status: string;
}

export interface TaskUpdateDto {
  title: string;
  description?: string;
  priority: string;
  dueDate?: string | null;
  assignedUserId?: string | null;
}