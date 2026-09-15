import { Component, Inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TaskService } from '../../../core/services/task.service';
import { TaskDto, TaskUpdateDto } from '../../../models/task.model';
import { ProjectMemberDto } from '../../../models/project.model';

export interface TaskEditDialogData {
  task: TaskDto;
  members: ProjectMemberDto[];
}

@Component({
  selector: 'app-task-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './task-edit-dialog.html',
  styleUrl: './task-edit-dialog.scss'
})
export class TaskEditDialogComponent {
  formData: TaskUpdateDto;
  isSaving = signal(false);
  errorMessage = signal<string | null>(null);

  members: ProjectMemberDto[];

  constructor(
    private dialogRef: MatDialogRef<TaskEditDialogComponent>,
    private taskService: TaskService,
    @Inject(MAT_DIALOG_DATA) public data: TaskEditDialogData
  ) {
    this.members = data.members;
    this.formData = {
      title: data.task.title,
      description: data.task.description ?? '',
      priority: data.task.priority,
      dueDate: data.task.dueDate,
      assignedUserId: data.task.assignedUserId
    };
  }

  onSave(): void {
    this.isSaving.set(true);
    this.errorMessage.set(null);

    this.taskService.updateTask(this.data.task.id, this.formData).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogRef.close(true);
      },
      error: () => {
        this.isSaving.set(false);
        this.errorMessage.set('تعذر حفظ التعديلات');
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}